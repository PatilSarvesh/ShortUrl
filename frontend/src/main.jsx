import { StrictMode, useEffect, useMemo, useRef, useState } from 'react';
import { createRoot } from 'react-dom/client';
import './styles.css';

const RECENT_STORAGE_KEY = 'shorturl.recent.v1';
const MAX_RECENT_LINKS = 8;

function isHttpUrl(value) {
  try {
    const url = new URL(value);
    return url.protocol === 'http:' || url.protocol === 'https:';
  } catch {
    return false;
  }
}

function isExpired(entry) {
  return Boolean(entry.isExpired || (entry.expiresAt && new Date(entry.expiresAt) <= new Date()));
}

function formatDate(value) {
  if (!value) return 'Just now';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Recently';
  return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

function formatExpiry(value) {
  if (!value) return 'Never expires';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Expiration unavailable';
  return `Expires ${formatDate(date)}`;
}

function readRecentLinks() {
  try {
    const entries = JSON.parse(localStorage.getItem(RECENT_STORAGE_KEY) || '[]');
    return Array.isArray(entries) ? entries : [];
  } catch {
    return [];
  }
}

async function copyText(value) {
  if (navigator.clipboard?.writeText) {
    await navigator.clipboard.writeText(value);
    return;
  }

  const temporaryInput = document.createElement('textarea');
  temporaryInput.value = value;
  temporaryInput.setAttribute('readonly', '');
  temporaryInput.style.position = 'fixed';
  temporaryInput.style.opacity = '0';
  document.body.appendChild(temporaryInput);
  temporaryInput.select();
  document.execCommand('copy');
  temporaryInput.remove();
}

function App() {
  const [url, setUrl] = useState('');
  const [customCode, setCustomCode] = useState('');
  const [expirationHours, setExpirationHours] = useState('');
  const [recentLinks, setRecentLinks] = useState(readRecentLinks);
  const [search, setSearch] = useState('');
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRefreshingStats, setIsRefreshingStats] = useState(false);
  const [activeNav, setActiveNav] = useState('create');
  const inputRef = useRef(null);
  const createRef = useRef(null);

  useEffect(() => {
    try {
      localStorage.setItem(RECENT_STORAGE_KEY, JSON.stringify(recentLinks));
    } catch {
      // Device-local history is optional; the API remains usable without storage.
    }
  }, [recentLinks]);

  const filteredLinks = useMemo(() => {
    const query = search.trim().toLowerCase();
    return recentLinks.filter((entry) =>
      !query || entry.shortUrl.toLowerCase().includes(query) || entry.destinationUrl.toLowerCase().includes(query));
  }, [recentLinks, search]);

  const totalClicks = recentLinks.reduce((total, entry) => total + (entry.clickCount ?? 0), 0);
  const activeLinks = recentLinks.filter((entry) => !isExpired(entry)).length;

  const saveLink = (entry) => {
    setRecentLinks((current) => [entry, ...current.filter((item) => item.shortCode !== entry.shortCode)].slice(0, MAX_RECENT_LINKS));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setResult(null);

    const normalizedUrl = url.trim();
    const normalizedCode = customCode.trim();
    if (!isHttpUrl(normalizedUrl)) {
      setError('Enter a valid URL beginning with http:// or https://.');
      inputRef.current?.focus();
      return;
    }

    if (normalizedCode && !/^[A-Za-z0-9_-]{4,32}$/.test(normalizedCode)) {
      setError('Custom codes use 4–32 letters, numbers, hyphens, or underscores.');
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await fetch('/api/url', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          url: normalizedUrl,
          customCode: normalizedCode || null,
          expirationHours: expirationHours ? Number(expirationHours) : null
        })
      });
      const data = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(data.message || 'Something went wrong. Please try again.');

      setResult(data);
      saveLink(data);
      setCustomCode('');
      createRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    } catch (submissionError) {
      setError(submissionError.message || 'Unable to create a short link right now.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const refreshStats = async (entry) => {
    try {
      const response = await fetch(`/api/url/${encodeURIComponent(entry.shortCode)}/stats`);
      if (!response.ok) throw new Error('Stats unavailable');
      const updated = await response.json();
      saveLink(updated);
      setResult((current) => current?.shortCode === updated.shortCode ? updated : current);
    } catch {
      setError('Stats are temporarily unavailable.');
    }
  };

  const handleResultStats = async () => {
    if (!result) return;
    setIsRefreshingStats(true);
    await refreshStats(result);
    setIsRefreshingStats(false);
  };

  const focusCreate = () => {
    setActiveNav('create');
    createRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    window.setTimeout(() => inputRef.current?.focus(), 350);
  };

  const clearRecent = () => {
    setRecentLinks([]);
    setSearch('');
  };

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="Primary navigation">
        <a className="brand" href="#create" aria-label="ShortUrl home">
          <span className="brand-mark" aria-hidden="true">↗</span>
          <span>ShortUrl</span>
        </a>

        <div className="workspace-switcher">
          <span className="workspace-avatar" aria-hidden="true">P</span>
          <span className="workspace-copy"><strong>Personal workspace</strong><small>Local workspace</small></span>
          <span className="workspace-chevron" aria-hidden="true">⌄</span>
        </div>

        <div className="nav-label">Workspace</div>
        <nav className="nav-list">
          <a className={`nav-item${activeNav === 'create' ? ' active' : ''}`} href="#create" onClick={() => setActiveNav('create')}><span className="nav-icon" aria-hidden="true">＋</span>Create link</a>
          <a className={`nav-item${activeNav === 'recent' ? ' active' : ''}`} href="#recent" onClick={() => setActiveNav('recent')}><span className="nav-icon" aria-hidden="true">☷</span>Recent links</a>
        </nav>

        <div className="sidebar-spacer" />
        <div className="sidebar-note"><span className="note-icon" aria-hidden="true">✦</span><strong>Keep it simple</strong><p>Your recent links stay on this device. No account required.</p></div>
        <div className="sidebar-footer"><span className="online-dot" aria-hidden="true" />MongoDB connected locally</div>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <a className="mobile-brand" href="#create" aria-label="ShortUrl home"><span className="brand-mark" aria-hidden="true">↗</span>ShortUrl</a>
          <div className="breadcrumbs"><span>Workspace</span><span aria-hidden="true">/</span><strong>Links</strong></div>
          <div className="top-actions"><span className="local-pill"><span className="online-dot" aria-hidden="true" />Local mode</span><button className="top-button" type="button" onClick={focusCreate}><span aria-hidden="true">＋</span> New link</button></div>
        </header>

        <main className="content">
          <section id="create" ref={createRef} className="page-intro" aria-labelledby="page-title">
            <div className="intro-copy"><div className="eyebrow">LINK WORKSPACE</div><h1 id="page-title">Share less.<br /><span>Reach more.</span></h1><p>Create clean, memorable links in a few seconds. Built for the links you share every day.</p></div>
            <div className="stats-grid" aria-label="Workspace overview">
              <article className="stat-card"><span className="stat-icon purple" aria-hidden="true">↗</span><div><span>Created here</span><strong>{recentLinks.length}</strong><small>On this device</small></div></article>
              <article className="stat-card"><span className="stat-icon green" aria-hidden="true">↯</span><div><span>Total clicks</span><strong>{totalClicks}</strong><small>Across your links</small></div></article>
              <article className="stat-card"><span className="stat-icon amber" aria-hidden="true">◷</span><div><span>Active links</span><strong>{activeLinks}</strong><small>Ready to share</small></div></article>
            </div>
          </section>

          <section className="workspace-grid" aria-label="Create a short link">
            <div className="panel create-panel">
              <div className="panel-heading"><div className="heading-icon" aria-hidden="true">↗</div><div><span className="panel-kicker">START HERE</span><h2>Create a new link</h2></div><span className="quiet-badge">No account required</span></div>
              <form className="shorten-form" onSubmit={handleSubmit} noValidate>
                <div className="field-group primary-field"><label htmlFor="url-input">Long URL</label><div className="input-wrap"><span className="input-icon" aria-hidden="true">↗</span><input ref={inputRef} id="url-input" name="url" type="url" inputMode="url" autoComplete="url" placeholder="https://your-long-link.com/…" value={url} onChange={(event) => setUrl(event.target.value)} required autoFocus /></div><span className="field-hint">Paste the full URL you want to share.</span></div>
                <div className="options-row"><div className="field-group"><label htmlFor="custom-code">Custom code <span>Optional</span></label><div className="option-input-wrap"><span aria-hidden="true">…/</span><input id="custom-code" name="customCode" type="text" maxLength="32" placeholder="my-link" autoComplete="off" spellCheck="false" value={customCode} onChange={(event) => setCustomCode(event.target.value)} /></div><span className="field-hint">4–32 letters, numbers, - or _</span></div><div className="field-group"><label htmlFor="expiration">Expiration</label><select id="expiration" name="expirationHours" value={expirationHours} onChange={(event) => setExpirationHours(event.target.value)}><option value="">Never expires</option><option value="24">After 1 day</option><option value="168">After 7 days</option><option value="720">After 30 days</option></select><span className="field-hint">You can keep it permanent.</span></div></div>
                {error && <p className="form-message error-message" role="alert">{error}</p>}
                <button className="primary-button" type="submit" disabled={isSubmitting}><span>{isSubmitting ? 'Creating link…' : 'Create short link'}</span><span className="button-arrow" aria-hidden="true">→</span></button>
              </form>

              {result && <section className="result-card" aria-live="polite"><div className="result-status"><span className="success-mark" aria-hidden="true">✓</span><span>Link created successfully</span></div><div className="result-link-row"><a className="short-url" href={result.shortUrl} target="_blank" rel="noreferrer">{result.shortUrl}</a><button className="copy-button" type="button" onClick={async () => { try { await copyText(result.shortUrl); } catch { setError('Copy failed. Select the link and copy it manually.'); } }}>Copy</button></div><div className="result-details"><span>Destination <strong title={result.destinationUrl}>{result.destinationUrl}</strong></span><span>Code <strong>{result.shortCode}</strong></span><span>{isExpired(result) ? 'Expired' : formatExpiry(result.expiresAt)}</span><span><strong>{result.clickCount ?? 0}</strong> clicks</span></div><div className="result-actions"><a href={result.shortUrl} target="_blank" rel="noreferrer">Open link <span aria-hidden="true">↗</span></a><button className="text-button" type="button" onClick={handleResultStats} disabled={isRefreshingStats}>{isRefreshingStats ? 'Refreshing…' : 'Refresh stats'}</button></div></section>}
            </div>

            <aside className="panel guide-panel" aria-label="How ShortUrl works"><div className="guide-top"><span className="panel-kicker">THE SIMPLE LOOP</span><span className="guide-spark" aria-hidden="true">✦</span></div><h2>From long to<br /><em>lightweight.</em></h2><div className="guide-steps"><div className="guide-step"><span>01</span><div><strong>Paste a link</strong><p>Bring the URL you want to share.</p></div></div><div className="guide-line" aria-hidden="true" /><div className="guide-step"><span>02</span><div><strong>Make it yours</strong><p>Choose a custom code or let us generate one.</p></div></div><div className="guide-line" aria-hidden="true" /><div className="guide-step"><span>03</span><div><strong>Share with confidence</strong><p>Copy, track clicks, and keep moving.</p></div></div></div></aside>
          </section>

          <section id="recent" className="recent-section" aria-labelledby="recent-heading"><div className="section-heading"><div><div className="eyebrow">YOUR LIBRARY</div><h2 id="recent-heading">Recent links</h2><p>Links created from this browser appear here.</p></div><div className="library-actions"><label className="search-wrap" htmlFor="recent-search"><span aria-hidden="true">⌕</span><input id="recent-search" type="search" placeholder="Search links…" value={search} onChange={(event) => setSearch(event.target.value)} /></label><button className="text-button" type="button" onClick={clearRecent}>Clear all</button></div></div><div className={`empty-state${filteredLinks.length ? ' hidden' : ''}`}><span className="empty-icon" aria-hidden="true">↗</span><h3>{recentLinks.length ? 'No links found' : 'Your link library is empty'}</h3><p>{recentLinks.length ? 'Try a different URL, code, or destination.' : 'Create your first short link above and it will show up here.'}</p>{!recentLinks.length && <a href="#create" className="empty-action" onClick={() => setActiveNav('create')}>Create a link <span aria-hidden="true">→</span></a>}</div><div className={`link-list${filteredLinks.length ? '' : ' hidden'}`}>{filteredLinks.map((entry) => <LinkRow key={entry.shortCode} entry={entry} onCopy={copyText} onStats={refreshStats} onError={setError} />)}</div></section>
        </main>

        <footer className="footer"><span>ShortUrl <b>•</b> Make links lighter.</span><span>Built for focused sharing.</span></footer>
      </div>
    </div>
  );
}

function LinkRow({ entry, onCopy, onStats, onError }) {
  const expired = isExpired(entry);
  const [isLoading, setIsLoading] = useState(false);
  const copy = async () => {
    try { await onCopy(entry.shortUrl); } catch { onError('Copy failed. Select the link and copy it manually.'); }
  };
  const stats = async () => {
    setIsLoading(true);
    await onStats(entry);
    setIsLoading(false);
  };

  return <article className="link-item"><div className="link-main"><div className="link-top"><a className="recent-url" href={entry.shortUrl} target="_blank" rel="noreferrer">{entry.shortUrl}</a><span className={`link-badge${expired ? ' expired' : ''}`}>{expired ? 'Expired' : 'Active'}</span></div><p className="recent-destination" title={entry.destinationUrl}>{entry.destinationUrl}</p><div className="recent-meta">{formatDate(entry.createdOn)} · {entry.clickCount ?? 0} clicks · {expired ? 'Expired' : (entry.expiresAt ? formatExpiry(entry.expiresAt) : 'Never expires')}</div></div><div className="link-actions"><button className="small-button" type="button" onClick={copy}>Copy</button><button className="small-button quiet-button" type="button" onClick={stats} disabled={isLoading}>{isLoading ? '…' : 'Stats'}</button><a className="open-button" href={entry.shortUrl} target="_blank" rel="noreferrer" title="Open link" aria-label={`Open ${entry.shortUrl}`}>↗</a></div></article>;
}

createRoot(document.getElementById('root')).render(<StrictMode><App /></StrictMode>);

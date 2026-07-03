import { useEffect, useState } from 'react';
import { api, type Invoice, type CommissionTotal } from './api';

function formatMoney(amount: number, currency: string): string {
  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
  }).format(amount);
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

export default function App() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [total, setTotal] = useState<CommissionTotal | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [inv, tot] = await Promise.all([
          api.getInvoices(),
          api.getCommissionTotal(),
        ]);
        if (!cancelled) {
          setInvoices(inv);
          setTotal(tot);
        }
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Something went wrong');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  const paidCount = invoices.filter((i) => i.status === 'Paid').length;
  const overdueCount = invoices.filter((i) => i.status === 'Overdue').length;
  const outstanding = invoices
    .filter((i) => i.status !== 'Paid')
    .reduce((sum, i) => sum + i.amount, 0);

  return (
    <div className="shell">
      <header className="topbar">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true" />
          <span className="brand-text">Invoice&#8202;<span className="brand-arrow">&rarr;</span>&#8202;Cash</span>
        </div>
        <span className="topbar-tag">Commission Console</span>
      </header>

      <main className="content">
        <div className="page-head">
          <h1>Receivables overview</h1>
          <p className="subtitle">Partner invoices and the commission they generate, in real time.</p>
        </div>

        {error && (
          <div className="notice notice-error" role="alert">
            <strong>Couldn&rsquo;t load data.</strong> {error}
            <span className="notice-hint">Is the API running on <code>http://localhost:5088</code>?</span>
          </div>
        )}

        {loading && !error && (
          <div className="notice">Loading receivables&hellip;</div>
        )}

        {!loading && !error && (
          <>
            <section className="stats" aria-label="Summary">
              <div className="stat stat-accent">
                <span className="stat-label">Commission earned</span>
                <span className="stat-value">
                  {total ? formatMoney(total.total, total.currency) : '—'}
                </span>
                <span className="stat-note">2.5% on paid invoices</span>
              </div>
              <div className="stat">
                <span className="stat-label">Outstanding</span>
                <span className="stat-value">{formatMoney(outstanding, 'PLN')}</span>
                <span className="stat-note">{invoices.length - paidCount} unpaid</span>
              </div>
              <div className="stat">
                <span className="stat-label">Paid</span>
                <span className="stat-value">{paidCount}</span>
                <span className="stat-note">of {invoices.length} invoices</span>
              </div>
              <div className="stat">
                <span className="stat-label">Overdue</span>
                <span className="stat-value stat-warn">{overdueCount}</span>
                <span className="stat-note">need attention</span>
              </div>
            </section>

            <section className="table-wrap" aria-label="Invoices">
              <table className="ledger">
                <thead>
                  <tr>
                    <th className="col-id">#</th>
                    <th>Partner</th>
                    <th>Status</th>
                    <th className="num">Amount</th>
                    <th className="num">Commission</th>
                    <th className="num">Due</th>
                  </tr>
                </thead>
                <tbody>
                  {invoices.map((inv) => (
                    <tr key={inv.id}>
                      <td className="col-id">{String(inv.id).padStart(2, '0')}</td>
                      <td className="partner">{inv.partnerName}</td>
                      <td>
                        <span className={`pill pill-${inv.status.toLowerCase()}`}>
                          {inv.status}
                        </span>
                      </td>
                      <td className="num">{formatMoney(inv.amount, inv.currency)}</td>
                      <td className="num commission">
                        {inv.commission > 0 ? formatMoney(inv.commission, inv.currency) : '—'}
                      </td>
                      <td className="num due">{formatDate(inv.dueDate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </section>
          </>
        )}
      </main>

      <footer className="foot">
        <span>.NET 10 API &middot; React + Vite &middot; typed REST contract</span>
      </footer>
    </div>
  );
}

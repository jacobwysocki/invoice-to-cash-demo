// Typed API client. In Squizzu these types are generated from Swagger;
// here they're hand-written to mirror the backend DTOs.

export type InvoiceStatus = 'Pending' | 'Paid' | 'Overdue';

export interface Invoice {
  id: number;
  partnerName: string;
  amount: number;
  currency: string;
  status: InvoiceStatus;
  dueDate: string;
  commission: number;
}

export interface CommissionTotal {
  total: number;
  currency: string;
}

const BASE = '/api';

async function getJson<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}${path}`);
  if (!res.ok) {
    throw new Error(`Request failed (${res.status}) for ${path}`);
  }
  return res.json() as Promise<T>;
}

export const api = {
  getInvoices: () => getJson<Invoice[]>('/invoices'),
  getCommissionTotal: () => getJson<CommissionTotal>('/invoices/commission-total'),
};

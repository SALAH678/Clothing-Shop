import React, { useState } from "react";
import { ChevronLeft, ChevronRight, RefreshCw } from "lucide-react";
import { useAdminPurchases } from "../hooks/useAdminQueries";
import { formatPriceDA } from "../../../lib/pricing";

const shortId = (id: string) => id.slice(0, 8).toUpperCase();

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString("fr-DZ", {
    year: "numeric",
    month: "short",
    day: "2-digit",
  });

const statusClasses = (status: string) => {
  const normalized = status.trim().toLowerCase();
  if (normalized === "paid") return "bg-green-100 text-green-900 border-green-900";
  if (normalized === "pending") return "bg-yellow-100 text-yellow-900 border-yellow-900";
  if (normalized === "failed") return "bg-red-100 text-red-900 border-red-900";
  return "bg-surface text-primary";
};

export const AdminPurchases: React.FC = () => {
  const [page, setPage] = useState(1);
  const { data, isPending, isError, isFetching, refetch } = useAdminPurchases(page);

  const purchases = data?.items ?? [];
  const totalPages = Math.max(1, data?.totalPages ?? 1);
  const totalCount = data?.totalCount ?? 0;

  return (
    <div className="flex flex-col gap-6">
      {isError && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">Failed to load purchases.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="bg-red-600 text-white font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-red-600 flex items-center gap-2 hover:bg-white hover:text-red-700 transition-colors cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" /> Retry
          </button>
        </div>
      )}

      <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] overflow-x-auto">
        <table className="w-full text-left font-mono border-collapse min-w-250">
          <thead className="bg-primary text-white">
            <tr>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Purchase ID</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Customer</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Contact & Address</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Items</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Total</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Status</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Date</th>
            </tr>
          </thead>
          <tbody className="bg-white text-sm font-bold divide-y-2 divide-primary">
            {isPending ? (
              <tr>
                <td colSpan={7} className="p-6 text-center text-secondary font-mono animate-pulse">
                  Loading purchases…
                </td>
              </tr>
            ) : purchases.length === 0 ? (
              <tr>
                <td colSpan={7} className="p-6 text-center text-secondary font-mono">
                  No purchases found.
                </td>
              </tr>
            ) : (
              purchases.map((order) => (
                <tr key={order.purchaseId} className="hover:bg-surface-container transition-colors items-start">
                  <td className="p-4 border-r-2 border-primary align-top text-secondary">
                    {shortId(order.purchaseId)}
                  </td>
                  <td className="p-4 border-r-2 border-primary align-top">{order.customerName}</td>
                  <td className="p-4 border-r-2 border-primary align-top">
                    <div className="flex flex-col gap-1">
                      <span>{order.customerPhoneNumber || "—"}</span>
                      <span className="font-normal text-xs text-secondary">{order.street}</span>
                      <span className="font-normal text-xs text-secondary">
                        {order.city}, {order.wilaya}
                      </span>
                    </div>
                  </td>
                  <td className="p-4 border-r-2 border-primary align-top">
                    <ul className="list-disc pl-4 font-normal text-xs flex flex-col gap-1">
                      {order.purchaseItems.map((item, idx) => (
                        <li key={idx}>
                          <span className="font-bold">{item.quantity}x</span> {item.productName}
                          <span className="text-secondary ml-1">({formatPriceDA(item.unitPrice)})</span>
                        </li>
                      ))}
                    </ul>
                  </td>
                  <td className="p-4 border-r-2 border-primary align-top">{formatPriceDA(order.totalAmount)}</td>
                  <td className="p-4 border-r-2 border-primary align-top">
                    <span
                      className={`px-2 py-1 border-2 border-primary whitespace-nowrap ${statusClasses(order.status)}`}
                    >
                      {order.status}
                    </span>
                  </td>
                  <td className="p-4 align-top whitespace-nowrap">{formatDate(order.date)}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
        <div className="min-w-250 p-4 border-t-4 border-primary bg-surface flex items-center justify-between gap-4">
          <span className="font-mono text-xs font-bold uppercase text-secondary">
            Page {data?.pageNumber ?? page} of {totalPages} — {totalCount} purchases
            {isFetching && <span className="ml-2 animate-pulse">loading…</span>}
          </span>
          <div className="sticky right-0 z-10 flex shrink-0 gap-2 bg-surface pl-4">
            <button
              type="button"
              disabled={page <= 1 || isFetching}
              onClick={() => setPage((prev) => Math.max(1, prev - 1))}
              className="bg-primary text-white font-mono font-bold uppercase py-2 px-4 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all active:translate-x-2 active:translate-y-2 active:shadow-none disabled:opacity-40 disabled:pointer-events-none flex items-center gap-1"
            >
              <ChevronLeft className="w-4 h-4" /> Prev
            </button>
            <button
              type="button"
              disabled={page >= totalPages || isFetching}
              onClick={() => setPage((prev) => Math.min(totalPages, prev + 1))}
              className="bg-primary text-white font-mono font-bold uppercase py-2 px-4 border-2 border-primary shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all active:translate-x-2 active:translate-y-2 active:shadow-none disabled:opacity-40 disabled:pointer-events-none flex items-center gap-1"
            >
              Next <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminPurchases;

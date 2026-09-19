import React, { useState } from "react";
import { ChevronLeft, ChevronRight, RefreshCw } from "lucide-react";
import { useAdminUsers } from "../hooks/useAdminQueries";

const shortId = (id: string) => id.slice(0, 8).toUpperCase();

export const AdminUsers: React.FC = () => {
  const [page, setPage] = useState(1);
  const { data, isPending, isError, isFetching, refetch } = useAdminUsers(page);

  const users = data?.items ?? [];
  const totalPages = Math.max(1, data?.totalPages ?? 1);
  const totalCount = data?.totalCount ?? 0;

  return (
    <div className="flex flex-col gap-6">
      {isError && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">Failed to load users.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="bg-red-600 text-white font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-red-600 flex items-center gap-2 hover:bg-white hover:text-red-700 transition-colors cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" /> Retry
          </button>
        </div>
      )}

      <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] overflow-hidden w-full">
        <div className="overflow-x-auto w-full">
          <table className="w-full text-left font-mono border-collapse min-w-225">
            <thead className="bg-primary text-white">
              <tr>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">ID</th>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">First Name</th>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Last Name</th>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Email</th>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Phone</th>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Role</th>
                <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Verified</th>
              </tr>
            </thead>
            <tbody className="bg-white text-sm font-bold divide-y-2 divide-primary">
              {isPending ? (
                <tr>
                  <td colSpan={7} className="p-6 text-center text-secondary font-mono animate-pulse">
                    Loading users…
                  </td>
                </tr>
              ) : users.length === 0 ? (
                <tr>
                  <td colSpan={7} className="p-6 text-center text-secondary font-mono">
                    No users found.
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <tr key={user.userId} className="hover:bg-surface-container transition-colors">
                    <td className="p-4 border-r-2 border-primary text-secondary">{shortId(user.userId)}</td>
                    <td className="p-4 border-r-2 border-primary">{user.firstName}</td>
                    <td className="p-4 border-r-2 border-primary">{user.lastName}</td>
                    <td className="p-4 border-r-2 border-primary font-normal">{user.email}</td>
                    <td className="p-4 border-r-2 border-primary font-normal">{user.phoneNumber || "—"}</td>
                    <td className="p-4 border-r-2 border-primary">
                      <span
                        className={`px-2 py-1 border-2 border-primary ${user.role === "Admin" ? "bg-black text-white" : "bg-surface"}`}
                      >
                        {user.role}
                      </span>
                    </td>
                    <td className="p-4">
                      <span
                        className={`px-2 py-1 border-2 border-primary ${user.isEmailVerified ? "bg-green-100 text-green-900 border-green-900" : "bg-red-100 text-red-900 border-red-900"}`}
                      >
                        {user.isEmailVerified ? "Yes" : "No"}
                      </span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        <div className="w-full p-4 border-t-4 border-primary bg-surface flex items-center justify-between gap-4">
          <span className="font-mono text-xs font-bold uppercase text-secondary">
            Page {data?.pageNumber ?? page} of {totalPages} — {totalCount} users
            {isFetching && <span className="ml-2 animate-pulse">loading…</span>}
          </span>
          <div className="flex shrink-0 gap-2">
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

export default AdminUsers;

import React from 'react';
import { Users, Package, Tags, ShoppingBag, RefreshCw } from 'lucide-react';
import { useDashboardOverview } from '../hooks/useAdminQueries';

export const AdminOverview: React.FC = () => {
  const { data, isPending, isError, refetch, isFetching } = useDashboardOverview();

  const stats = [
    { label: 'Total Users', value: data?.totalUsers, icon: Users },
    { label: 'Total Products', value: data?.totalProducts, icon: Package },
    { label: 'Categories', value: data?.totalCategories, icon: Tags },
    { label: 'Total Purchases', value: data?.totalPurchases, icon: ShoppingBag },
  ];

  return (
    <div className="flex flex-col gap-6">
      {isError && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">
            Failed to load dashboard overview.
          </p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="bg-red-600 text-white font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-red-600 flex items-center gap-2 hover:bg-white hover:text-red-700 transition-colors cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" /> Retry
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat, idx) => (
          <div
            key={idx}
            className="bg-white border-4 border-primary p-6 shadow-[8px_8px_0_0_#000] flex flex-col gap-4 transition-transform hover:-translate-y-2 duration-300"
          >
            <div className="flex justify-between items-start">
              <span className="font-mono text-sm font-bold uppercase text-secondary">
                {stat.label}
              </span>
              <stat.icon className="w-6 h-6 text-primary" />
            </div>
            <span className="font-display text-5xl font-black">
              {isPending ? '…' : (stat.value ?? 0)}
            </span>
          </div>
        ))}
      </div>

      {isFetching && !isPending && (
        <p className="font-mono text-[10px] font-bold uppercase text-secondary animate-pulse">
          Refreshing…
        </p>
      )}
    </div>
  );
};

export default AdminOverview;

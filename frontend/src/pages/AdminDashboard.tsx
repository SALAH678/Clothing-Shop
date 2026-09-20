import React from 'react';
import { Link, Navigate, useParams } from 'react-router-dom';
import { LayoutDashboard, Users, Package, Tags, ShoppingBag, Home } from 'lucide-react';
import { AdminOverview } from '../features/admin/components/AdminOverview';
import { AdminUsers } from '../features/admin/components/AdminUsers';
import { AdminProducts } from '../features/admin/components/AdminProducts';
import { AdminCategories } from '../features/admin/components/AdminCategories';
import { AdminPurchases } from '../features/admin/components/AdminPurchases';

export type { AdminTab } from '../features/admin/types/admin';
import { adminTabPath, isAdminTab, type AdminTab } from '../features/admin/types/admin';

const navItems: { id: AdminTab; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { id: 'overview', label: 'Overview', icon: LayoutDashboard },
  { id: 'users', label: 'Users', icon: Users },
  { id: 'products', label: 'Products', icon: Package },
  { id: 'categories', label: 'Categories', icon: Tags },
  { id: 'purchases', label: 'Purchases', icon: ShoppingBag },
];

export default function AdminDashboard() {
  // The active tab lives in the URL (`/admin/:tab`) so tabs are deep-linkable,
  // survive a refresh and work with the browser back/forward buttons.
  const { tab } = useParams<{ tab: string }>();

  // `/admin` with no segment (and any unknown segment) falls back to the
  // canonical overview URL instead of rendering an empty panel.
  if (!isAdminTab(tab)) return <Navigate to={adminTabPath('overview')} replace />;

  const activeTab = tab;

  const activeLabel =
    activeTab === 'overview'
      ? 'Dashboard Overview'
      : (navItems.find((item) => item.id === activeTab)?.label ?? activeTab);

  return (
    <div className="min-h-screen bg-surface flex flex-col md:flex-row border-t-4 border-primary">
      {/* Sidebar Navigation */}
      <aside className="w-full md:w-64 bg-white border-b-4 md:border-b-0 md:border-r-4 border-primary shrink-0 z-10 relative shadow-[8px_0_0_0_#000]">
        <div className="p-6 border-b-4 border-primary bg-primary text-white">
          <h2 className="font-display text-2xl font-black uppercase tracking-tighter">Admin Panel</h2>
        </div>
        <nav className="flex flex-row md:flex-col p-4 gap-2 overflow-x-auto md:overflow-visible">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = activeTab === item.id;
            return (
              <Link
                key={item.id}
                to={adminTabPath(item.id)}
                aria-current={isActive ? 'page' : undefined}
                className={`flex items-center gap-3 px-4 py-3 font-mono font-bold uppercase transition-all duration-300 border-2 whitespace-nowrap ${
                  isActive
                    ? 'bg-primary text-white border-primary translate-x-1 translate-y-1 shadow-none'
                    : 'bg-surface text-primary border-primary shadow-[4px_4px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:shadow-none hover:bg-surface-container'
                }`}
              >
                <Icon className="w-5 h-5 shrink-0" />
                <span>{item.label}</span>
              </Link>
            );
          })}
        </nav>
      </aside>

      {/* Main Content Area — each tab fetches its own data from the API */}
      <main className="flex-1 min-w-0 p-4 sm:p-6 md:p-10 overflow-y-auto">
        <div className="w-full">
          {/* Header */}
          <div className="mb-8 border-b-4 border-primary pb-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <h1 className="font-display text-4xl font-black uppercase tracking-tighter">
              {activeLabel}
            </h1>
            <Link
              to="/"
              title="Back to the storefront"
              className="bg-surface text-primary font-mono text-xs font-bold uppercase py-2.5 px-4 border-2 border-primary flex items-center justify-center gap-2 shrink-0 w-full sm:w-auto whitespace-nowrap shadow-[3px_3px_0_0_#000] hover:shadow-[1px_1px_0_0_#000] hover:translate-x-0.5 hover:translate-y-0.5 hover:bg-white transition-all cursor-pointer"
            >
              <Home className="w-4 h-4" /> Back to Home
            </Link>
          </div>

          {/* TAB CONTENT */}
          {activeTab === 'overview' && <AdminOverview />}
          {activeTab === 'users' && <AdminUsers />}
          {activeTab === 'products' && <AdminProducts />}
          {activeTab === 'categories' && <AdminCategories />}
          {activeTab === 'purchases' && <AdminPurchases />}
        </div>
      </main>
    </div>
  );
}
interface PageLoaderProps {
  label?: string;
}

/**
 * Route-level suspense fallback. Rendered inside the layouts (never above them)
 * so the Navbar/Footer stay mounted while a lazily-loaded page chunk downloads.
 */
export default function PageLoader({ label = "Loading" }: PageLoaderProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      className="grow flex flex-col items-center justify-center gap-4 p-16 min-h-[50vh]"
    >
      <span className="font-mono text-xs font-bold uppercase tracking-widest animate-pulse">
        {label}...
      </span>
      <span className="w-32 h-1 bg-secondary/20 overflow-hidden">
        <span className="block h-full w-1/2 bg-primary animate-pulse" />
      </span>
    </div>
  );
}
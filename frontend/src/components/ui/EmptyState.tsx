interface EmptyStateProps {
  title?: string;
  message?: string;
}

export default function EmptyState({
  title = "Nothing found",
  message = "There is nothing available to display right now.",
}: EmptyStateProps) {
  return (
    <main className="flex min-h-[50vh] items-center justify-center border-b border-primary bg-surface px-6 py-20">
      <section
        aria-labelledby="empty-state-title"
        className="w-full max-w-2xl border border-primary bg-surface-container p-8 shadow-[8px_8px_0_0_rgba(0,0,0,1)] md:p-12"
      >
        <div className="border-b border-primary pb-6">
          <p className="mb-3 font-mono text-xs font-bold uppercase tracking-[0.25em] text-secondary">
            No results
          </p>

          <h1
            id="empty-state-title"
            className="font-display text-3xl font-extrabold uppercase tracking-tight md:text-5xl"
          >
            {title}
          </h1>
        </div>

        <p className="mt-8 max-w-xl font-body text-base leading-relaxed text-secondary md:text-lg">
          {message}
        </p>
      </section>
    </main>
  );
}
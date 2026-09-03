import { AlertTriangle, RefreshCw } from "lucide-react";

interface ErrorStateProps {
  title?: string;
  label?: string;
  message?: string;
  onRetry?: () => unknown;
}

export default function ErrorState({
  title = "Something went wrong",
  label = "Error / Unable to load data",
  message = "We could not load the requested data. Please try again in a moment.",
  onRetry,
}: ErrorStateProps) {
  return (
    <main className="flex min-h-[70vh] items-center justify-center border-b border-primary bg-surface px-6 py-20">
      <section
        aria-labelledby="error-state-title"
        className="w-full max-w-2xl border border-primary bg-surface-container p-8 shadow-[8px_8px_0_0_rgba(0,0,0,1)] md:p-12"
      >
        <div className="mb-8 flex items-start justify-between gap-6 border-b border-primary pb-6">
          <div>
            <p className="mb-3 font-mono text-xs font-bold uppercase tracking-[0.25em] text-secondary">
              {label}
            </p>

            <h1
              id="error-state-title"
              className="font-display text-3xl font-extrabold uppercase tracking-tight md:text-5xl"
            >
              {title}
            </h1>
          </div>

          <AlertTriangle
            aria-hidden="true"
            className="h-10 w-10 shrink-0"
            strokeWidth={1.5}
          />
        </div>

        <p className="max-w-xl font-body text-base leading-relaxed text-secondary md:text-lg">
          {message}
        </p>

        {onRetry && (
          <button
            type="button"
            onClick={() => void onRetry()}
            className="mt-8 inline-flex items-center gap-3 border border-primary bg-primary px-6 py-4 font-mono text-xs font-bold uppercase tracking-[0.2em] text-on-primary transition-all hover:-translate-y-1 hover:bg-surface hover:text-primary hover:shadow-[5px_5px_0_0_rgba(0,0,0,1)] active:translate-x-0.5 active:translate-y-0.5 active:shadow-none"
          >
            <RefreshCw aria-hidden="true" className="h-4 w-4" />
            Try again
          </button>
        )}
      </section>
    </main>
  );
}
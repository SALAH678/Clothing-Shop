import { useEffect, useState } from "react";
import { isAxiosError } from "axios";

/** True when the API rejected the request with 429 Too Many Requests. */
export function isRateLimitError(error: unknown): boolean {
  return isAxiosError(error) && error.response?.status === 429;
}

/**
 * Seconds until the 429 block lifts, read from the Retry-After response header.
 * The backend sends integer seconds; an HTTP-date value is also handled per RFC 9110.
 * Returns null when the header is missing (callers fall back to a sensible default).
 */
export function getRetryAfterSeconds(error: unknown): number | null {
  if (!isAxiosError(error) || !error.response) return null;

  const raw = error.response.headers?.["retry-after"];
  if (raw == null || raw === "") return null;

  if (/^\d+$/.test(String(raw))) {
    return Math.max(0, parseInt(String(raw), 10));
  }

  const retryDate = Date.parse(String(raw));
  if (!Number.isNaN(retryDate)) {
    return Math.max(0, Math.ceil((retryDate - Date.now()) / 1000));
  }

  return null;
}

/** Human message for a 429, or `fallback` when the error is anything else. */
export function getRateLimitMessage(error: unknown, fallback: string): string {
  if (!isRateLimitError(error)) return fallback;

  const seconds = getRetryAfterSeconds(error) ?? 60;
  return `Too many attempts. Please try again in ${seconds} second${seconds === 1 ? "" : "s"}.`;
}

export interface RateLimitState {
  /** A 429 block is currently active — the UI should disable submissions. */
  active: boolean;
  /** Seconds until the block lifts (0 when not active). */
  secondsRemaining: number;
  /** Ready-to-render countdown message, or null when not active. */
  message: string | null;
}

/**
 * Turns a 429 mutation error into a live countdown for the UI.
 *
 * When a 429 error object is seen, the block duration is armed from its
 * Retry-After header (60s fallback). A 1s ticker then counts it down, so
 * the timer survives react-query re-renders and keeps running while the
 * user is idle.
 *
 * This is a reaction, not a defense: the backend limiters are the real
 * gate — this only helps the user understand *when* to retry.
 */
export function useRateLimit(error: unknown): RateLimitState {
  // Duration of the current block in seconds; 0 when no block is armed.
  const [duration, setDuration] = useState(0);
  // Whole seconds elapsed since the current block started.
  const [elapsed, setElapsed] = useState(0);
  const [prevError, setPrevError] = useState<unknown>(null);

  // Adjust state during render when the incoming error changes — the React
  // documented pattern for reacting to prop changes without effect cascades.
  // Purely derived from props (header value), so no impure calls are needed:
  // the block starts at t=0 and the interval below does the ticking.
  // react-query hands back a stable error object per attempt, so this guard
  // re-arms exactly once per failed request and never loops.
  if (error !== prevError) {
    setPrevError(error);
    const seconds = isRateLimitError(error) ? (getRetryAfterSeconds(error) ?? 60) : 0;
    setDuration(seconds);
    setElapsed(0);
  }

  const active = duration > 0 && elapsed < duration;

  // Tick once per second while a block is active so the countdown runs live.
  // setState fires inside the interval callback (an external event), never
  // synchronously in the effect body, satisfying the hooks lint rules.
  useEffect(() => {
    if (duration <= 0 || elapsed >= duration) return;

    const timer = setInterval(() => {
      setElapsed((prev) => Math.min(prev + 1, duration));
    }, 1000);
    return () => clearInterval(timer);
  }, [duration, elapsed]);

  const secondsRemaining = active ? duration - elapsed : 0;

  return {
    active,
    secondsRemaining,
    message: active ? `Too many attempts. Please try again in ${secondsRemaining}s.` : null,
  };
}


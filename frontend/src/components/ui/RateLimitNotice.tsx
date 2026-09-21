interface RateLimitNoticeProps {
  /** Live countdown message from `useRateLimit` — renders nothing while null. */
  message: string | null;
}

/** Banner shown while a 429 rate-limit block from the backend is active. */
export default function RateLimitNotice({ message }: RateLimitNoticeProps) {
  if (!message) return null;

  return (
    <div
      role="alert"
      className="p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase"
    >
      {message}
    </div>
  );
}

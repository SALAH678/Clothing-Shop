/**
 * Decodes the payload of a JWT access token (no signature verification —
 * this is only used client-side to read non-sensitive claims like the role).
 * Returns null if the token is malformed.
 */
export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const [, payload] = token.split(".");
    if (!payload) return null;

    // JWT uses base64url encoding; convert it to standard base64 for atob().
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");
    const bytes = Uint8Array.from(atob(padded), (char) => char.charCodeAt(0));
    return JSON.parse(new TextDecoder().decode(bytes)) as Record<string, unknown>;
  } catch {
    return null;
  }
}

// ASP.NET Core emits the role under this default claim name unless a custom
// JwtSecurityTokenHandler claim mapping override (e.g. "role") is configured.
const CLAIM_TYPES_ROLE = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export type AuthRole = "Admin" | "Customer";

/**
 * Extracts the user's role from a JWT access token.
 * Handles both the ASP.NET Core default claim name and a plain "role" claim,
 * case-insensitively. Returns null when the token has no recognizable role.
 */
export function getRoleFromAccessToken(token: string | null | undefined): AuthRole | null {
  if (!token) return null;

  const payload = decodeJwtPayload(token);
  if (!payload) return null;

  const claim = payload["role"] ?? payload[CLAIM_TYPES_ROLE];
  if (typeof claim !== "string") return null;

  const normalized = claim.toLowerCase();
  if (normalized === "admin") return "Admin";
  if (normalized === "customer") return "Customer";
  return null;
}

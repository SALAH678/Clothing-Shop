const API_URL = (import.meta.env.VITE_API_URL as string | undefined ?? "").replace(/\/$/, "");

export function resolveImageUrl(imageUrl: string | null | undefined): string | undefined {
  if (!imageUrl) return undefined;
  if (/^https?:\/\//i.test(imageUrl)) return imageUrl;
  if (!API_URL) return `/${imageUrl.replace(/^\//, "")}`;
  return `${API_URL}/${imageUrl.replace(/^\//, "")}`;
}

export function getApiUrl(): string {
  return API_URL;
}
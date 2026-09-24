import { Phone, Map, Navigation } from "lucide-react";

// Virage Luxe — Bab Ezzouar, Algiers
// Source: https://www.google.com/maps/place/Virage+Luxe/@36.7204593,3.1817723,17z
const LAT = 36.7204593;
const LNG = 3.1817723;
const PLACE_NAME = "Virage Luxe";
const ADDRESS_LABEL = "Bab Ezzouar, Algiers, Algeria";

// Link that opens the exact place in Google Maps (new tab).
const GOOGLE_MAPS_URL = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(
  `${PLACE_NAME} ${LAT},${LNG}`
)}`;

// Directions link (destination = store coordinates).
const GOOGLE_MAPS_DIRECTIONS_URL = `https://www.google.com/maps/dir/?api=1&destination=${LAT},${LNG}`;

// Optional Embed API key. If `VITE_GOOGLE_MAPS_API_KEY` is set we use the
// official Maps Embed API (`/maps/embed/v1/place`), otherwise we fall back to
// the keyless `output=embed` iframe which needs no billing / API key.
const MAPS_EMBED_API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY as
  | string
  | undefined;

const MAPS_EMBED_SRC = MAPS_EMBED_API_KEY
  ? `https://www.google.com/maps/embed/v1/place?key=${MAPS_EMBED_API_KEY}&q=${encodeURIComponent(
      `${PLACE_NAME}, Bab Ezzouar, Algiers`
    )}&center=${LAT},${LNG}&zoom=17`
  : `https://maps.google.com/maps?q=${encodeURIComponent(
      `${PLACE_NAME} ${LAT},${LNG}`
    )}&t=&z=17&ie=UTF8&iwloc=&output=embed`;

export default function StoreLocator() {
  return (
    <section className="border-t border-primary bg-surface-container py-12 md:py-16 lg:py-24">
      <div className="px-6 md:px-16 max-w-[1600px] mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-12 gap-12 md:gap-6">
          <div className="md:col-span-4 flex flex-col justify-center border-b md:border-b-0 md:border-r border-primary pb-12 md:pb-0 md:pr-8 lg:pr-12">
            <h2 className="font-display text-2xl md:text-3xl lg:text-5xl font-bold uppercase mb-6 lg:mb-8 tracking-tighter">
              Where to Find Us
            </h2>
            <div className="mb-6 lg:mb-8">
              <h3 className="font-display text-lg md:text-xl lg:text-2xl font-bold uppercase mb-2 tracking-tight">
                Virage Luxe
              </h3>
              <p className="font-body text-sm md:text-base lg:text-lg text-secondary">
                Style and comfort Oversize — {ADDRESS_LABEL}.
              </p>
              <p className="font-mono text-[10px] md:text-xs mt-2 uppercase tracking-widest text-secondary">
                {LAT.toFixed(7)}, {LNG.toFixed(7)}
              </p>
            </div>

            <div className="space-y-4 lg:space-y-6">
              <a href="tel:0776105085" className="flex items-center gap-3 lg:gap-4 group w-fit">
                <span className="border border-primary p-2 lg:p-3 group-hover:bg-primary group-hover:text-white transition-all duration-300 bg-surface shadow-[4px_4px_0_0_rgba(0,0,0,1)] group-hover:-translate-y-1 group-hover:-translate-x-1 group-hover:shadow-[6px_6px_0_0_rgba(0,0,0,1)]">
                  <Phone className="w-4 h-4 lg:w-5 lg:h-5" />
                </span>
                <span className="font-mono text-[10px] md:text-xs lg:text-sm font-bold text-primary uppercase tracking-widest group-hover:translate-x-2 transition-transform duration-300">
                  0776 10 50 85
                </span>
              </a>

              <a
                href={GOOGLE_MAPS_URL}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-3 lg:gap-4 group w-fit"
              >
                <span className="border border-primary p-2 lg:p-3 group-hover:bg-primary group-hover:text-white transition-all duration-300 bg-surface shadow-[4px_4px_0_0_rgba(0,0,0,1)] group-hover:-translate-y-1 group-hover:-translate-x-1 group-hover:shadow-[6px_6px_0_0_rgba(0,0,0,1)]">
                  <Map className="w-4 h-4 lg:w-5 lg:h-5" />
                </span>
                <span className="font-mono text-[10px] md:text-xs lg:text-sm font-bold text-primary uppercase tracking-widest group-hover:translate-x-2 transition-transform duration-300">
                  Open in Google Maps
                </span>
              </a>

              <a
                href={GOOGLE_MAPS_DIRECTIONS_URL}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-3 lg:gap-4 group w-fit"
              >
                <span className="border border-primary p-2 lg:p-3 group-hover:bg-primary group-hover:text-white transition-all duration-300 bg-surface shadow-[4px_4px_0_0_rgba(0,0,0,1)] group-hover:-translate-y-1 group-hover:-translate-x-1 group-hover:shadow-[6px_6px_0_0_rgba(0,0,0,1)]">
                  <Navigation className="w-4 h-4 lg:w-5 lg:h-5" />
                </span>
                <span className="font-mono text-[10px] md:text-xs lg:text-sm font-bold text-primary uppercase tracking-widest group-hover:translate-x-2 transition-transform duration-300">
                  Get Directions — Bab Ezzouar
                </span>
              </a>
            </div>
          </div>

          <div className="md:col-span-8 min-h-[280px] h-62.5 md:h-75 lg:h-112.5 border border-primary relative overflow-hidden group shadow-xl hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1 transition-all duration-300 bg-surface">
            <iframe
              title={`${PLACE_NAME} — ${ADDRESS_LABEL} on Google Maps`}
              src={MAPS_EMBED_SRC}
              className="absolute inset-0 h-full w-full border-0 grayscale group-hover:grayscale-0 transition-all duration-700"
              loading="lazy"
              allowFullScreen
              referrerPolicy="no-referrer-when-downgrade"
            />
            <a
              href={GOOGLE_MAPS_URL}
              target="_blank"
              rel="noopener noreferrer"
              className="absolute bottom-4 left-4 font-mono text-[10px] md:text-xs font-bold text-white bg-black/80 px-4 py-2 uppercase tracking-widest backdrop-blur-sm border border-white/20 hover:bg-black transition-colors"
            >
              {PLACE_NAME} — View larger map
            </a>
          </div>
        </div>
      </div>
    </section>
  );
}

import { Phone, Map } from "lucide-react";

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
                Clothing Shop
              </h3>
              <p className="font-body text-sm md:text-base lg:text-lg text-secondary">Style and comfort Oversize.</p>
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

              <a href="#" className="flex items-center gap-3 lg:gap-4 group w-fit">
                <span className="border border-primary p-2 lg:p-3 group-hover:bg-primary group-hover:text-white transition-all duration-300 bg-surface shadow-[4px_4px_0_0_rgba(0,0,0,1)] group-hover:-translate-y-1 group-hover:-translate-x-1 group-hover:shadow-[6px_6px_0_0_rgba(0,0,0,1)]">
                  <Map className="w-4 h-4 lg:w-5 lg:h-5" />
                </span>
                <span className="font-mono text-[10px] md:text-xs lg:text-sm font-bold text-primary uppercase tracking-widest group-hover:translate-x-2 transition-transform duration-300">
                  Open in Google Maps
                </span>
              </a>
            </div>
          </div>

          <div className="md:col-span-8 h-62.5 md:h-75 lg:h-112.5 border border-primary relative overflow-hidden group shadow-xl hover:shadow-2xl hover:shadow-black/50 hover:-translate-y-1 transition-all duration-300 bg-surface">
            <div
              className="absolute inset-0 bg-cover bg-center grayscale opacity-80 group-hover:grayscale-0 group-hover:opacity-100 transition-all duration-700"
              style={{
                backgroundImage:
                  'url("https://images.unsplash.com/photo-1524661135-423995f22d0b?auto=format&fit=crop&q=80")',
              }}
            />
            <div className="absolute inset-0 bg-black/10 transition-colors group-hover:bg-transparent"></div>
            <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
              <span className="font-mono text-sm font-bold text-white bg-black/80 px-6 py-3 uppercase tracking-widest backdrop-blur-sm border border-white/20">
                [ Google Maps Integration ]
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

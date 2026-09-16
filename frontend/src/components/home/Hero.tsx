import { Link } from "react-router-dom";

// Above-the-fold hero art. Override with `VITE_HERO_IMAGE_URL` (a self-hosted
// `/hero.webp` is strongly preferred) so the landing page never depends on a
// third-party CDN URL that can expire.
const HERO_IMAGE_URL: string =
  import.meta.env.VITE_HERO_IMAGE_URL ??
  "https://lh3.googleusercontent.com/aida-public/AB6AXuAt90_xb8iwT_I-40c7M0cUYPrnHeyhh1k0soG_UEi38muNMi9oJckkDl5dfReaadlJA9t-xRqCQDqRE2Yr7T70tV6L_vv_34KCVTtUkuCQL5SJZ1sPrTO0144ICggxUIKiusYsKqF1fQX-0Dl1fY-OedDWIjJSq4ZdrZxke_yxqdrNLzMNgKdvVTfSwq_eGP9MW4RKHqIjRq7o4uojrarGcQA6u-gEX2fCoK8KFrHRhtqmLcCkGUPZEg";

export default function Hero() {
  return (
    <section className="relative w-full h-[80vh] flex items-center justify-center border-b border-primary bg-surface-container overflow-hidden">
      {/* Fallback layer keeps the hero legible even if the image fails to load. */}
      <div className="absolute inset-0 z-0 bg-linear-to-br from-surface-container-high to-surface-container" />
      <div
        aria-hidden="true"
        className="absolute inset-0 z-0 opacity-60 grayscale bg-cover bg-center"
        style={{ backgroundImage: `url("${HERO_IMAGE_URL}")` }}
      />
      <div className="absolute inset-0 z-0 bg-black/20" />
      <div className="relative z-10 text-center px-6 mt-12 md:mt-0">
        <h1 className="font-display text-5xl md:text-7xl lg:text-8xl font-extrabold uppercase text-white drop-shadow-lg mb-6 tracking-tighter mix-blend-overlay">
          Style & Comfort
          <br />
          Oversize
        </h1>
        <Link 
          to="/categories" 
          className="inline-block bg-primary text-on-primary font-mono text-sm uppercase py-4 px-8 border border-primary tracking-widest font-bold shadow-xl transition-all duration-300 ease-out hover:-translate-y-2 hover:shadow-2xl hover:bg-white hover:text-black active:translate-y-1 active:scale-95 active:shadow-md"
        >
          Shop All
        </Link>
      </div>
    </section>
  );
}

import { Link } from "react-router-dom";
import heroImage from "../../assets/hero.jpg";

// Above-the-fold hero art, bundled from `src/assets/hero.jpg` so the landing
// page never depends on a third-party CDN URL that can expire. Override with
// `VITE_HERO_IMAGE_URL` if a different image is ever needed.
const HERO_IMAGE_URL: string = import.meta.env.VITE_HERO_IMAGE_URL ?? heroImage;

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

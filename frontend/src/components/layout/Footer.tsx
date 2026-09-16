import { Link } from "react-router-dom";

interface FooterLink {
  label: string;
  href?: string;
  external?: boolean;
}

// Privacy/Terms have no pages yet, so they render as non-navigating text rather
// than `href="#"`, which would scroll the user back to the top of the page.
const footerLinks: FooterLink[] = [
  { label: "Contact", href: "mailto:bourouchemohammedsalah@gmail.com" },
  {
    label: "Google Maps",
    href: "https://www.google.com/maps/search/?api=1&query=Clothing+Shop",
    external: true,
  },
  { label: "Privacy" },
  { label: "Terms" },
];

export default function Footer() {
  return (
    <footer className="bg-primary w-full px-6 md:px-16 py-12 md:py-24">
      <div className="grid grid-cols-1 md:grid-cols-12 gap-8 w-full max-w-[1600px] mx-auto">
        <div className="md:col-span-4">
          <div className="font-display text-2xl font-black text-on-primary uppercase mb-4 tracking-tighter">
            Clothing Shop
          </div>
          <p className="font-body text-zinc-400 text-sm">
            © {new Date().getFullYear()} CLOTHING SHOP. ALL RIGHTS RESERVED.
          </p>
        </div>

        <div className="md:col-span-8 flex flex-col sm:flex-row flex-wrap gap-6 sm:gap-8 md:justify-end sm:items-center">
          {footerLinks.map((link) =>
            link.href ? (
              link.external ? (
                <a
                  key={link.label}
                  href={link.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="font-mono text-sm font-bold text-zinc-400 hover:text-on-primary transition-colors uppercase tracking-widest"
                >
                  {link.label}
                </a>
              ) : (
                <Link
                  key={link.label}
                  to={link.href}
                  className="font-mono text-sm font-bold text-zinc-400 hover:text-on-primary transition-colors uppercase tracking-widest"
                >
                  {link.label}
                </Link>
              )
            ) : (
              <span
                key={link.label}
                aria-disabled="true"
                title="Coming soon"
                className="font-mono text-sm font-bold text-zinc-600 uppercase tracking-widest cursor-not-allowed"
              >
                {link.label}
              </span>
            ),
          )}
        </div>
      </div>

      <div className="w-full max-w-[1600px] mx-auto mt-16 pt-8 border-t border-zinc-800 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <p className="font-mono text-[10px] sm:text-xs font-bold text-zinc-500 uppercase tracking-widest">
          Created by: Bourouchoue Mohamed Salah
        </p>
        <a
          href="mailto:bourouchemohammedsalah@gmail.com"
          className="font-mono text-[10px] sm:text-xs font-bold text-zinc-500 hover:text-white transition-colors uppercase tracking-widest"
        >
          Gmail: bourouchemohammedsalah@gmail.com
        </a>
      </div>
    </footer>
  );
}

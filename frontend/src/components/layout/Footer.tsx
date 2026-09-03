export default function Footer() {
  return (
    <footer className="bg-primary w-full px-6 md:px-16 py-12 md:py-24">
      <div className="grid grid-cols-1 md:grid-cols-12 gap-8 w-full max-w-[1600px] mx-auto">
        <div className="md:col-span-4">
          <div className="font-display text-2xl font-black text-on-primary uppercase mb-4 tracking-tighter">
            Clothing Shop
          </div>
          <p className="font-body text-zinc-400 text-sm">© 2024 CLOTHING SHOP. ALL RIGHTS RESERVED.</p>
        </div>

        <div className="md:col-span-8 flex flex-col sm:flex-row flex-wrap gap-6 sm:gap-8 md:justify-end sm:items-center">
          {["Contact", "Google Maps", "Privacy", "Terms"].map((link) => (
            <a
              key={link}
              href="#"
              className="font-mono text-sm font-bold text-zinc-400 hover:text-on-primary transition-colors uppercase tracking-widest"
            >
              {link}
            </a>
          ))}
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

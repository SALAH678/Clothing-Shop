import { Search, ShoppingCart, User } from "lucide-react";
import { Link } from "react-router-dom";

export default function Navbar() {
  return (
    <header className="bg-surface sticky top-0 z-50 border-b border-primary flex flex-col md:flex-row justify-between items-center w-full px-4 md:px-4 lg:px-6 py-4 transition-all duration-300 gap-4 md:gap-6 lg:gap-8">
      <div className="flex items-center justify-between w-full md:w-auto shrink-0">
        <Link
          to="/"
          className="font-display text-xl md:text-base lg:text-2xl font-bold uppercase tracking-tighter text-primary whitespace-nowrap pr-2"
        >
          Clothing Shop
        </Link>
        <div className="flex gap-4 md:hidden items-center">
          <div className="group relative flex items-center justify-end h-6">
            <Search className="w-6 h-6 cursor-pointer z-10" />
            <input
              type="text"
              placeholder="Search..."
              className="absolute right-0 w-0 opacity-0 group-hover:w-32 group-hover:opacity-100 focus:w-32 focus:opacity-100 transition-all duration-300 ease-out bg-surface border-b-2 border-transparent group-hover:border-primary focus:border-primary font-mono text-xs py-1 pr-8 outline-none z-0 cursor-text"
            />
          </div>
          <ShoppingCart className="w-6 h-6 hover:opacity-70 transition-opacity cursor-pointer" />
          <User className="w-6 h-6 hover:opacity-70 transition-opacity cursor-pointer" />
        </div>
      </div>
      <nav className="hidden md:flex gap-2 md:gap-3 lg:gap-6 items-center flex-nowrap justify-center flex-1 min-w-0">
        {["T-Shirt Oversize", "Pants", "Old Money Shirts", "Sneakers", "Ultra Baggy"].map((item, i, arr) => (
          <div key={item} className="flex items-center gap-2 md:gap-3 lg:gap-6">
            <Link
              to={`/categories/${item.toLowerCase().trim().replace(/\s+/g, "-")}`}
              className="relative group inline-block active:scale-95 transition-transform duration-150"
            >
              <span className="text-secondary font-mono text-[9px] md:text-[10px] lg:text-sm uppercase font-bold tracking-widest whitespace-nowrap block">
                {item}
              </span>
              <span
                aria-hidden="true"
                className="absolute top-0 left-0 text-primary font-mono text-[9px] md:text-[10px] lg:text-sm uppercase font-bold tracking-widest whitespace-nowrap overflow-hidden w-0 group-hover:w-full transition-all duration-300 ease-out"
              >
                {item}
              </span>
              <span className="absolute -bottom-1 left-0 w-full h-0.5 bg-primary origin-left scale-x-0 group-hover:scale-x-100 transition-transform duration-300 ease-out"></span>
            </Link>
            {i < arr.length - 1 && <span className="text-zinc-300">|</span>}
          </div>
        ))}
      </nav>
      <div className="hidden md:flex gap-4 lg:gap-6 items-center shrink-0">
        <div className="group relative flex items-center justify-end h-5 lg:h-6">
          <Search className="w-4 h-4 lg:w-5 lg:h-5 cursor-pointer z-10 hover:scale-110 transition-transform" />
          <input
            type="text"
            placeholder="Search product..."
            className="absolute right-0 w-0 opacity-0 group-hover:w-35 lg:group-hover:w-50 group-hover:opacity-100 focus:w-35 lg:focus:w-50 focus:opacity-100 transition-all duration-300 ease-out bg-surface border-b-2 border-transparent group-hover:border-primary focus:border-primary font-mono text-[10px] lg:text-xs py-1 pr-6 lg:pr-8 outline-none z-0 cursor-text"
          />
        </div>
        <User className="w-4 h-4 lg:w-5 lg:h-5 hover:scale-110 transition-transform cursor-pointer" />
        <ShoppingCart className="w-4 h-4 lg:w-5 lg:h-5 hover:scale-110 transition-transform cursor-pointer" />
      </div>
    </header>
  );
}

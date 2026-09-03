import { Truck, BadgeCheck, Diamond } from "lucide-react";

export default function Features() {
  return (
    <section className="grid grid-cols-1 md:grid-cols-3 border-b border-primary">
      <div className="flex flex-col items-center justify-center py-16 px-8 border-b md:border-b-0 md:border-r border-primary hover:bg-surface transition-all duration-300 text-center group hover:shadow-[8px_8px_0_0_rgba(0,0,0,1)] hover:-translate-y-1 hover:-translate-x-1 relative z-10 hover:z-20 bg-surface-container-lowest">
        <Truck className="w-10 h-10 mb-4 stroke-[1.5] group-hover:scale-110 transition-transform" />
        <h3 className="font-display text-2xl font-bold uppercase mb-2 tracking-tight">Shipping to 69 Wilayas</h3>
        <p className="font-mono text-sm text-secondary uppercase tracking-widest">Everywhere in Algeria</p>
      </div>
      <div className="flex flex-col items-center justify-center py-16 px-8 border-b md:border-b-0 md:border-r border-primary hover:bg-surface transition-all duration-300 text-center group hover:shadow-[8px_8px_0_0_rgba(0,0,0,1)] hover:-translate-y-1 hover:-translate-x-1 relative z-10 hover:z-20 bg-surface-container-lowest">
        <BadgeCheck className="w-10 h-10 mb-4 stroke-[1.5] group-hover:scale-110 transition-transform" />
        <h3 className="font-display text-2xl font-bold uppercase mb-2 tracking-tight">Premium Quality</h3>
        <p className="font-mono text-sm text-secondary uppercase tracking-widest">High-End Fabrics</p>
      </div>
      <div className="flex flex-col items-center justify-center py-16 px-8 hover:bg-surface transition-all duration-300 text-center group hover:shadow-[8px_8px_0_0_rgba(0,0,0,1)] hover:-translate-y-1 hover:-translate-x-1 relative z-10 hover:z-20 bg-surface-container-lowest">
        <Diamond className="w-10 h-10 mb-4 stroke-[1.5] group-hover:scale-110 transition-transform" />
        <h3 className="font-display text-2xl font-bold uppercase mb-2 tracking-tight">Unique Models</h3>
        <p className="font-mono text-sm text-secondary uppercase tracking-widest">Exclusive Designs</p>
      </div>
    </section>
  );
}

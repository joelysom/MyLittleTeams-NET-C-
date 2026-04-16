import Link from 'next/link';

export default function Navbar() {
  return (
    <nav className="sticky top-0 z-50 border-b border-white/10 bg-slate-950/95 shadow-sm shadow-slate-950/10 backdrop-blur-xl">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
        <Link href="/" className="inline-flex items-center gap-3 text-white">
          <span className="inline-flex h-10 w-10 items-center justify-center rounded-2xl bg-gradient-to-br from-cyan-500 to-indigo-600 text-lg font-black shadow-lg shadow-cyan-500/20">
            C
          </span>
          <span className="text-lg font-semibold tracking-tight">Choas</span>
        </Link>

        <div className="hidden items-center gap-8 sm:flex">
          <Link href="#features" className="text-sm font-medium text-slate-300 transition hover:text-white">
            Recursos
          </Link>
          <Link href="#contact" className="text-sm font-medium text-slate-300 transition hover:text-white">
            Contato
          </Link>
        </div>

        <div className="hidden sm:block">
          <Link
            href="#contact"
            className="inline-flex items-center rounded-full bg-cyan-500 px-5 py-2.5 text-sm font-semibold text-slate-950 shadow-lg shadow-cyan-500/20 transition hover:bg-cyan-400"
          >
            Entrar
          </Link>
        </div>
      </div>
    </nav>
  );
}

import Link from 'next/link';
import { Home } from 'lucide-react';

export default function Navbar() {
  return (
    <nav className="sticky top-0 z-50 border-b border-white/10 bg-slate-950/95 shadow-sm shadow-slate-950/10 backdrop-blur-xl">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
        <Link href="/" className="inline-flex items-center gap-3 text-white hover:opacity-80 transition">
          <span className="inline-flex h-10 w-10 items-center justify-center rounded-2xl bg-gradient-to-br from-cyan-500 to-indigo-600 shadow-lg shadow-cyan-500/20">
            <img src="/img/tesseractICO.png" alt="Choas Logo" className="h-6 w-6" />
          </span>
          <span className="text-lg font-semibold tracking-tight">Choas</span>
        </Link>

        <div className="hidden items-center gap-8 sm:flex">
          <Link href="/#features" className="text-sm font-medium text-slate-300 transition hover:text-white">
            Recursos
          </Link>
          <Link href="/#contact" className="text-sm font-medium text-slate-300 transition hover:text-white">
            Contato
          </Link>
        </div>

        <div className="flex items-center gap-3">
          <Link
            href="/"
            className="inline-flex items-center justify-center p-2 rounded-full hover:bg-white/10 transition text-slate-300 hover:text-white"
            title="Ir para home"
          >
            <Home size={20} />
          </Link>
          <Link
            href="/login"
            className="hidden sm:inline-flex items-center rounded-full bg-cyan-500 px-5 py-2.5 text-sm font-semibold text-slate-950 shadow-lg shadow-cyan-500/20 transition hover:bg-cyan-400"
          >
            Entrar
          </Link>
        </div>
      </div>
    </nav>
  );
}

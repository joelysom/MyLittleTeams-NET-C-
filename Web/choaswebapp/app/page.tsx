export default function Home() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <section className="relative overflow-hidden pb-24">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(56,189,248,0.18),_transparent_25%),radial-gradient(circle_at_bottom_right,_rgba(168,85,247,0.16),_transparent_20%)]" />
        <div className="relative mx-auto max-w-7xl px-4 pt-24 sm:px-6 lg:px-8">
          <div className="grid gap-12 xl:grid-cols-[1.2fr_0.8fr] xl:items-center">
            <div className="max-w-2xl">
              <div className="inline-flex rounded-full bg-white/10 px-4 py-2 text-sm font-semibold uppercase tracking-[0.22em] text-cyan-200 ring-1 ring-white/10 backdrop-blur">
                Observatório Acadêmico
              </div>
              <h1 className="mt-8 text-5xl font-extrabold tracking-tight text-white sm:text-6xl">
                Choas
              </h1>
              <p className="mt-4 max-w-2xl text-lg leading-8 text-slate-300 sm:text-xl">
                Observatório de Projetos Integradores para equipes, turmas e orientadores. Um workspace acadêmico moderno para gerenciar entregas, comunicação e progresso com clareza.
              </p>

              <div className="mt-10 flex flex-col gap-4 sm:flex-row">
                <a
                  href="#features"
                  className="inline-flex items-center justify-center rounded-full bg-cyan-500 px-6 py-3 text-sm font-semibold text-slate-950 shadow-lg shadow-cyan-500/20 transition hover:bg-cyan-400"
                >
                  Conheça o Choas
                </a>
                <a
                  href="#contact"
                  className="inline-flex items-center justify-center rounded-full border border-white/15 bg-white/5 px-6 py-3 text-sm font-semibold text-white transition hover:border-cyan-300 hover:bg-slate-800/80"
                >
                  Entrar na plataforma
                </a>
              </div>

              <div className="mt-12 grid gap-4 sm:grid-cols-3">
                <div className="rounded-3xl bg-white/5 p-5 ring-1 ring-white/10 backdrop-blur">
                  <p className="text-2xl font-semibold text-white">+4x</p>
                  <p className="mt-2 text-sm text-slate-400">Produtividade acadêmica</p>
                </div>
                <div className="rounded-3xl bg-white/5 p-5 ring-1 ring-white/10 backdrop-blur">
                  <p className="text-2xl font-semibold text-white">+120</p>
                  <p className="mt-2 text-sm text-slate-400">Projetos integradores monitorados</p>
                </div>
                <div className="rounded-3xl bg-white/5 p-5 ring-1 ring-white/10 backdrop-blur">
                  <p className="text-2xl font-semibold text-white">100%</p>
                  <p className="mt-2 text-sm text-slate-400">Foco em times e entrega colaborativa</p>
                </div>
              </div>
            </div>

            <div className="rounded-[2rem] border border-white/10 bg-slate-900/70 p-8 shadow-2xl shadow-slate-950/50 backdrop-blur-xl">
              <div className="flex items-center gap-4 rounded-3xl bg-slate-800/80 px-5 py-4 ring-1 ring-white/10">
                <div className="inline-flex h-12 w-12 items-center justify-center rounded-2xl bg-cyan-500 text-slate-950 text-lg font-black">C</div>
                <div>
                  <p className="text-sm uppercase tracking-[0.22em] text-cyan-300">Choas</p>
                  <p className="text-base font-semibold text-white">Observatório de Projetos Integradores</p>
                </div>
              </div>
              <div className="mt-8 space-y-6 text-slate-300">
                <div className="rounded-3xl bg-slate-950/80 p-5 ring-1 ring-white/5">
                  <p className="text-sm uppercase tracking-[0.16em] text-slate-400">Workspace</p>
                  <p className="mt-3 text-lg font-semibold text-white">Controle turmas, equipes e entregas em um só lugar.</p>
                </div>
                <div className="rounded-3xl bg-slate-950/80 p-5 ring-1 ring-white/5">
                  <p className="text-sm uppercase tracking-[0.16em] text-slate-400">Conexão</p>
                  <p className="mt-3 text-lg font-semibold text-white">Comunicação integrada com histórico e contexto acadêmico.</p>
                </div>
                <div className="rounded-3xl bg-slate-950/80 p-5 ring-1 ring-white/5">
                  <p className="text-sm uppercase tracking-[0.16em] text-slate-400">Organização</p>
                  <p className="mt-3 text-lg font-semibold text-white">Visual moderno e conteúdo alinhado ao ritmo do semestre.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section id="features" className="mx-auto max-w-7xl px-4 pb-24 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl text-center">
          <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-300">Recursos</p>
          <h2 className="mt-4 text-3xl font-bold tracking-tight text-white sm:text-4xl">
            Visual elegante, controle acadêmico real.
          </h2>
          <p className="mt-4 text-base leading-7 text-slate-300">
            Inspirado no desktop Choas, o landing page agora reflete design sofisticado, clareza de informação e a experiência de gestão de projetos integradores.
          </p>
        </div>

        <div className="mt-12 grid gap-6 md:grid-cols-2 xl:grid-cols-4">
          <div className="rounded-[1.75rem] border border-white/10 bg-slate-900/80 p-8 shadow-lg shadow-slate-950/20 backdrop-blur">
            <p className="text-3xl">📚</p>
            <h3 className="mt-6 text-xl font-semibold text-white">Gestão acadêmica</h3>
            <p className="mt-3 text-sm leading-6 text-slate-400">
              Centralize turmas, matrículas e entregas com a mesma disciplina de fluxo do Choas desktop.
            </p>
          </div>
          <div className="rounded-[1.75rem] border border-white/10 bg-slate-900/80 p-8 shadow-lg shadow-slate-950/20 backdrop-blur">
            <p className="text-3xl">🤝</p>
            <h3 className="mt-6 text-xl font-semibold text-white">Equipes e colaboração</h3>
            <p className="mt-3 text-sm leading-6 text-slate-400">
              Pingue seu time, compartilhe arquivos e mantenha o contexto do projeto em um painel único.
            </p>
          </div>
          <div className="rounded-[1.75rem] border border-white/10 bg-slate-900/80 p-8 shadow-lg shadow-slate-950/20 backdrop-blur">
            <p className="text-3xl">🧠</p>
            <h3 className="mt-6 text-xl font-semibold text-white">Insights e status</h3>
            <p className="mt-3 text-sm leading-6 text-slate-400">
              Acompanhe risco acadêmico, progresso e entrega com indicadores práticos e visuais.
            </p>
          </div>
          <div className="rounded-[1.75rem] border border-white/10 bg-slate-900/80 p-8 shadow-lg shadow-slate-950/20 backdrop-blur">
            <p className="text-3xl">🚀</p>
            <h3 className="mt-6 text-xl font-semibold text-white">Interface moderna</h3>
            <p className="mt-3 text-sm leading-6 text-slate-400">
              Tela limpa, tipografia forte e componentes que combinam com o estilo do app desktop Choas.
            </p>
          </div>
        </div>
      </section>

      <section id="contact" className="mx-auto max-w-7xl px-4 pb-24 sm:px-6 lg:px-8">
        <div className="rounded-[2rem] border border-white/10 bg-slate-900/80 p-10 shadow-2xl shadow-slate-950/30 backdrop-blur-xl">
          <div className="grid gap-10 xl:grid-cols-[1.1fr_0.9fr] xl:items-center">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.24em] text-cyan-300">Convite</p>
              <h2 className="mt-4 text-3xl font-bold tracking-tight text-white sm:text-4xl">
                Leve a experiência Choas para a web.
              </h2>
              <p className="mt-4 text-base leading-7 text-slate-300">
                Um landing page moderno e alinhado à identidade acadêmica do seu projeto. Mais do que uma página de apresentação, agora a web também transmite a sensação de um workspace confiável e focado em produtividade.
              </p>
            </div>
            <div className="space-y-4">
              <div className="rounded-3xl bg-white/5 p-6 ring-1 ring-white/10">
                <p className="text-sm text-slate-400">Plataforma</p>
                <p className="mt-2 font-semibold text-white">Choas | Observatório de Projetos Integradores</p>
              </div>
              <div className="rounded-3xl bg-white/5 p-6 ring-1 ring-white/10">
                <p className="text-sm text-slate-400">Design</p>
                <p className="mt-2 font-semibold text-white">Glassmorphism suave com gradientes e layouts estruturados.</p>
              </div>
              <div className="rounded-3xl bg-white/5 p-6 ring-1 ring-white/10">
                <p className="text-sm text-slate-400">Foco</p>
                <p className="mt-2 font-semibold text-white">Equipe, entrega e contexto acadêmico em primeiro lugar.</p>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

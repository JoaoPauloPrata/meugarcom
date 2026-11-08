import { useEffect, useState } from 'react'
import Logo from './components/Logo'
import Demo from './components/Demo'
import WaitlistForm from './components/WaitlistForm'

/* Ícones de traço (estilo Lucide) */
const ICONS = {
  chat: 'M21 12a8 8 0 0 1-11.6 7.1L4 20l1-4.6A8 8 0 1 1 21 12z',
  qr: 'M4 4h6v6H4zM14 4h6v6h-6zM4 14h6v6H4zM14 14h2v2h-2zM18 14h2v2h-2zM14 18h2v2h-2zM18 18h2v2h-2z',
  card: 'M3 6h18v12H3zM3 10h18M7 15h4',
  calendar: 'M4 5h16v15H4zM4 10h16M9 3v4M15 3v4',
  chart: 'M4 20V10M10 20V4M16 20v-7M22 20H2',
  bell: 'M6 16V11a6 6 0 1 1 12 0v5l1.5 2h-15zM10 21h4',
  heart: 'M12 20s-7-4.5-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.5-7 10-7 10z',
  globe: 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM3 12h18M12 3c2.5 2.5 3.5 5.5 3.5 9s-1 6.5-3.5 9c-2.5-2.5-3.5-5.5-3.5-9s1-6.5 3.5-9z',
  plug: 'M9 3v5M15 3v5M6 8h12v3a6 6 0 0 1-12 0zM12 17v4',
  phone: 'M7 3h10v18H7zM11 18h2',
  shield: 'M12 3l8 3v6c0 4.5-3.4 8-8 9-4.6-1-8-4.5-8-9V6z M9 12l2 2 4-4',
}

function Icon({ name, size = 20 }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d={ICONS[name]} />
    </svg>
  )
}

const FEATURES = [
  {
    icon: 'chat',
    title: 'Garçom de IA',
    text: 'Responde sobre pratos, ingredientes, alergias e harmonização — só com base no seu cardápio real. Nunca inventa prato.',
  },
  {
    icon: 'qr',
    title: 'Cardápio digital',
    text: 'Com a cara da sua casa: logo, cores e fotos. O cliente aponta a câmera para o QR code e pronto, sem baixar app.',
  },
  {
    icon: 'card',
    title: 'Pedido e Pix na mesa',
    text: 'Cada pessoa pede do próprio celular numa comanda compartilhada. Na hora de ir, divide a conta e paga por Pix ou cartão.',
  },
  {
    icon: 'calendar',
    title: 'Reservas',
    text: 'Reservas ligadas ao mapa do salão. Menos ligação, menos mesa vazia, menos fila na porta no sábado à noite.',
  },
  {
    icon: 'chart',
    title: 'Painel de dados',
    text: 'Veja o que os clientes perguntam, quais restrições aparecem mais, os pratos mais procurados e os horários de pico.',
  },
  {
    icon: 'bell',
    title: 'App da equipe',
    text: 'Pedidos chegam organizados por mesa para salão e cozinha. Seu time para de correr atrás de comanda.',
  },
]

const STEPS = [
  { n: '01', title: 'Envie seu cardápio', text: 'PDF, foto do cardápio impresso, link, planilha ou importação do seu PDV.' },
  { n: '02', title: 'A gente organiza', text: 'Pratos, preços, descrições e alérgenos viram um cardápio digital estruturado.' },
  { n: '03', title: 'Revise e personalize', text: 'Ajuste o que quiser, escolha o modelo, sua logo e suas cores. Publique.' },
  { n: '04', title: 'QR code na mesa', text: 'Imprima, coloque nas mesas e o garçom de IA começa a atender na hora.' },
]

const DIFFS = [
  { icon: 'globe', title: 'Fala como o cliente fala', text: '“Tem algo sem glúten?”, “qual é o mais pedido?”, “dá pra tirar a cebola?” — entende português do dia a dia, gíria e erro de digitação. E atende turista em inglês e espanhol.' },
  { icon: 'plug', title: 'Funciona com o seu PDV', text: 'Não precisa trocar de sistema. O Meu Garçom trabalha ao lado do caixa que você já usa.' },
  { icon: 'phone', title: 'Sem app, sem cadastro', text: 'Abre direto no navegador do celular. Atrito zero para o cliente.' },
  { icon: 'shield', title: 'Responde só o que é verdade', text: 'Toda resposta vem do seu cardápio. Se a informação não existe, o garçom chama alguém da equipe.' },
]

const FAQ = [
  {
    q: 'O garçom de IA substitui minha equipe?',
    a: 'Não. Ele tira dúvidas repetitivas e agiliza pedidos para que sua equipe tenha mais tempo para o que importa: receber bem, servir e resolver o que precisa de gente.',
  },
  {
    q: 'E se o cliente perguntar algo que não está no cardápio?',
    a: 'O garçom responde só com base nas informações que você cadastrou. Se não souber, ele avisa e pode chamar alguém da equipe para a mesa.',
  },
  {
    q: 'Preciso trocar meu sistema de caixa?',
    a: 'Não. O Meu Garçom funciona ao lado do PDV que você já usa. Integrações com os sistemas mais usados no Brasil estão no nosso roteiro.',
  },
  {
    q: 'Quanto tempo leva para colocar no ar?',
    a: 'A ideia é que você saia do cardápio em PDF para o QR code na mesa em cerca de 20 minutos.',
  },
  {
    q: 'Quanto vai custar?',
    a: 'Vai ter um plano gratuito com cardápio digital e garçom de IA para uma unidade. Os recursos avançados ficam no plano Pro — e quem entra na lista de espera garante condição especial de lançamento.',
  },
]

function useReveal() {
  useEffect(() => {
    const els = document.querySelectorAll('.reveal')
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      els.forEach((el) => el.classList.add('is-in'))
      return
    }
    const io = new IntersectionObserver(
      (entries) =>
        entries.forEach((e) => {
          if (e.isIntersecting) {
            e.target.classList.add('is-in')
            io.unobserve(e.target)
          }
        }),
      { threshold: 0.12, rootMargin: '0px 0px -40px 0px' },
    )
    els.forEach((el) => io.observe(el))
    return () => io.disconnect()
  }, [])
}

function Header() {
  const [scrolled, setScrolled] = useState(false)
  const [open, setOpen] = useState(false)
  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 8)
    onScroll()
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [])
  const close = () => setOpen(false)
  return (
    <header className={`header ${scrolled ? 'header--scrolled' : ''}`}>
      <div className="container header-inner">
        <Logo />
        <nav className={`nav ${open ? 'nav--open' : ''}`} aria-label="Principal">
          <a href="#produto" onClick={close}>Produto</a>
          <a href="#como-funciona" onClick={close}>Como funciona</a>
          <a href="#planos" onClick={close}>Planos</a>
          <a href="#duvidas" onClick={close}>Dúvidas</a>
          <a href="#lista" className="btn btn--primary btn--sm nav-cta-mobile" onClick={close}>Entrar na lista</a>
        </nav>
        <a href="#lista" className="btn btn--primary btn--sm header-cta">Entrar na lista</a>
        <button
          type="button"
          className="menu-btn"
          aria-label={open ? 'Fechar menu' : 'Abrir menu'}
          aria-expanded={open}
          onClick={() => setOpen((o) => !o)}
        >
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" aria-hidden="true">
            {open ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 8h16M4 16h16" />}
          </svg>
        </button>
      </div>
    </header>
  )
}

export default function App() {
  useReveal()

  return (
    <>
      <a href="#conteudo" className="skip-link">Pular para o conteúdo</a>
      <Header />

      <main id="conteudo">
        {/* HERO */}
        <section className="hero" id="topo">
          <div className="container hero-inner">
            <span className="eyebrow">Lista de espera aberta</span>
            <h1>
              Cada cliente <em>bem atendido</em>,<br /> mesmo com a casa cheia.
            </h1>
            <p className="lead">
              Cardápio digital por QR code com um garçom de IA que tira dúvidas sobre pratos, ingredientes e alergias — direto do seu
              cardápio real. Pedido, Pix e reservas na mesa, sem app.
            </p>
            <div className="hero-actions">
              <a href="#lista" className="btn btn--primary btn--lg">Entrar na lista de espera</a>
              <a href="#como-funciona" className="btn btn--ghost btn--lg">Ver como funciona</a>
            </div>
            <p className="hero-note">Plano gratuito para 1 unidade · Sem cartão de crédito</p>
          </div>
          <div className="container">
            <Demo />
          </div>
        </section>

        {/* FAIXA */}
        <section className="strip" aria-label="Destaques">
          <div className="container strip-inner">
            <span>Sem app para baixar</span>
            <span>Português de verdade</span>
            <span>Funciona com seu PDV</span>
            <span>Pix na mesa</span>
            <span>No ar em ~20 minutos</span>
          </div>
        </section>

        {/* PROBLEMA */}
        <section className="section">
          <div className="container problem reveal">
            <span className="eyebrow">O problema</span>
            <h2>
              “Esse prato tem lactose?” <span className="muted">“Qual serve duas pessoas?”</span> “Demora muito?”
            </h2>
            <p className="lead">
              Sexta à noite, salão cheio, e a equipe gastando tempo respondendo as mesmas perguntas mesa por mesa. Enquanto isso, o pedido
              atrasa, a conta demora e o cliente vai embora com a sensação de que foi esquecido.
            </p>
          </div>
        </section>

        {/* PRODUTO */}
        <section className="section section--tint" id="produto">
          <div className="container">
            <div className="section-head reveal">
              <span className="eyebrow">Produto</span>
              <h2>Tudo que acontece na mesa, num QR code.</h2>
              <p className="lead">Do “o que você recomenda?” até o Pix, o Meu Garçom cuida do caminho — e a sua equipe cuida das pessoas.</p>
            </div>
            <div className="features">
              {FEATURES.map((f, i) => (
                <article key={f.title} className="feature reveal" style={{ '--d': `${i * 60}ms` }}>
                  <span className="feature-icon">
                    <Icon name={f.icon} />
                  </span>
                  <h3>{f.title}</h3>
                  <p>{f.text}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        {/* COMO FUNCIONA */}
        <section className="section" id="como-funciona">
          <div className="container">
            <div className="section-head reveal">
              <span className="eyebrow">Como funciona</span>
              <h2>Do cardápio em PDF ao QR code na mesa em cerca de 20 minutos.</h2>
            </div>
            <ol className="steps">
              {STEPS.map((s, i) => (
                <li key={s.n} className="step reveal" style={{ '--d': `${i * 80}ms` }}>
                  <span className="step-n">{s.n}</span>
                  <h3>{s.title}</h3>
                  <p>{s.text}</p>
                </li>
              ))}
            </ol>
          </div>
        </section>

        {/* DIFERENCIAIS */}
        <section className="section section--dark">
          <div className="container">
            <div className="section-head reveal">
              <span className="eyebrow eyebrow--light">Feito para o Brasil</span>
              <h2>Hospitalidade de verdade, não automação fria.</h2>
            </div>
            <div className="diffs">
              {DIFFS.map((d, i) => (
                <article key={d.title} className="diff reveal" style={{ '--d': `${i * 60}ms` }}>
                  <Icon name={d.icon} size={22} />
                  <div>
                    <h3>{d.title}</h3>
                    <p>{d.text}</p>
                  </div>
                </article>
              ))}
            </div>
          </div>
        </section>

        {/* PLANOS */}
        <section className="section" id="planos">
          <div className="container">
            <div className="section-head reveal">
              <span className="eyebrow">Planos</span>
              <h2>Comece de graça. Cresça quando quiser.</h2>
            </div>
            <div className="plans">
              <article className="plan reveal">
                <h3>Gratuito</h3>
                <p className="plan-price">R$ 0</p>
                <p className="plan-sub">Para 1 unidade, para sempre</p>
                <ul>
                  <li>Cardápio digital com sua marca</li>
                  <li>Garçom de IA</li>
                  <li>QR code para as mesas</li>
                  <li>Sem cartão de crédito</li>
                </ul>
              </article>
              <article className="plan plan--pro reveal" style={{ '--d': '80ms' }}>
                <div className="plan-badge">Condição de lançamento</div>
                <h3>Pro</h3>
                <p className="plan-price plan-price--soon">Preço especial para a lista</p>
                <p className="plan-sub">Por unidade, sem limite de unidades</p>
                <ul>
                  <li>Tudo do Gratuito</li>
                  <li>Pedido e pagamento por Pix na mesa</li>
                  <li>Reservas com mapa do salão</li>
                  <li>Perfis de clientes e campanhas</li>
                  <li>Integração com WhatsApp</li>
                  <li>Painel de dados completo</li>
                </ul>
                <a href="#lista" className="btn btn--light">Garantir minha vaga</a>
              </article>
            </div>
          </div>
        </section>

        {/* LISTA DE ESPERA */}
        <section className="section section--tint" id="lista">
          <div className="container waitlist">
            <div className="waitlist-copy reveal">
              <span className="eyebrow">Lista de espera</span>
              <h2>Seja um dos primeiros restaurantes com o Meu Garçom.</h2>
              <p className="lead">Vamos liberar o acesso aos poucos, por ordem de inscrição. Quem entra agora ajuda a moldar o produto e garante condição especial.</p>
              <ul className="checks">
                <li>Acesso antecipado antes do lançamento</li>
                <li>Configuração assistida com nosso time</li>
                <li>Preço de lançamento no plano Pro</li>
              </ul>
            </div>
            <div className="waitlist-card reveal" style={{ '--d': '80ms' }}>
              <WaitlistForm />
            </div>
          </div>
        </section>

        {/* FAQ */}
        <section className="section" id="duvidas">
          <div className="container faq-wrap">
            <div className="section-head reveal">
              <span className="eyebrow">Dúvidas</span>
              <h2>Perguntas frequentes</h2>
            </div>
            <div className="faq reveal">
              {FAQ.map((f) => (
                <details key={f.q}>
                  <summary>
                    {f.q}
                    <span className="faq-plus" aria-hidden="true" />
                  </summary>
                  <p>{f.a}</p>
                </details>
              ))}
            </div>
          </div>
        </section>
      </main>

      <footer className="footer">
        <div className="container footer-inner">
          <Logo />
          <p>Cardápio digital com garçom de IA para restaurantes brasileiros.</p>
          <p className="footer-copy">© {new Date().getFullYear()} Meu Garçom</p>
        </div>
      </footer>
    </>
  )
}

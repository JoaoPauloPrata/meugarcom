export default function Logo({ className = '' }) {
  return (
    <a href="#topo" className={`logo ${className}`} aria-label="Meu Garçom — início">
      <img src="/logo-icon.png" alt="" width="50" height="20" />
      <span>Meu Garçom</span>
    </a>
  )
}

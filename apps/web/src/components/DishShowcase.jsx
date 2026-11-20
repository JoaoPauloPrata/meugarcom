import Icon from './Icon'

/** Decorative spinning dish with floating info chips (same motion as the product video). */
export default function DishShowcase({ className = '' }) {
  return (
    <div className={`dish-showcase ${className}`} aria-hidden="true">
      <div className="showcase-dish">
        <img src="/pratos/pizza-margherita.webp" alt="" width="300" height="300" />
      </div>
      <span className="float-chip float-chip--photo">
        <Icon name="chat" size={16} strokeWidth={2.2} />
        Foto do prato
      </span>
      <span className="float-chip float-chip--price">
        <Icon name="check" size={16} strokeWidth={2.6} />
        R$ 59
      </span>
      <span className="float-chip float-chip--ingredients">
        <Icon name="heart" size={16} strokeWidth={2.2} />
        Tomate, búfala, manjericão
      </span>
    </div>
  )
}

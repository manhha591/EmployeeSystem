interface CardProps {
  label: string
  value: string | number
  color: string
}

function Card({ label, value, color }: CardProps) {
  const borderColor = { "--card-color": color } as React.CSSProperties

  return (
    <div
      className="flex-1 p-5 rounded-lg text-center border"
      style={{ borderColor: color, ...borderColor }}
    >
      <div className="text-3xl font-bold" style={{ color }}>{value}</div>
      <div className="mt-2 text-gray-500">{label}</div>
    </div>
  )
}

export default Card

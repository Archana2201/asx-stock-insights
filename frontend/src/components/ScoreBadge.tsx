import { scoreColour } from '../format';

interface Props {
  score: number | null;
  rating?: string | null;
}

export function ScoreBadge({ score, rating }: Props) {
  if (score === null) return <span className="muted">-</span>;
  return (
    <span className="badge" style={{ backgroundColor: scoreColour(score) }}>
      {score.toFixed(0)}
      {rating ? ` · ${rating}` : ''}
    </span>
  );
}

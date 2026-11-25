import { scoreColour } from '../format';
import type { Recommendation } from '../types';
import { ScoreBadge } from './ScoreBadge';

interface Props {
  recommendation: Recommendation;
}

export function FactorBreakdown({ recommendation }: Props) {
  return (
    <section className="card">
      <h3>
        Score breakdown <ScoreBadge score={recommendation.totalScore} rating={recommendation.rating} />
      </h3>
      <p className="muted">{recommendation.summary}</p>
      <ul className="factors">
        {recommendation.factors.map((factor) => (
          <li key={factor.name}>
            <div className="factor-head">
              <span>
                <strong>{factor.name}</strong> <span className="muted">weight {(factor.weight * 100).toFixed(0)}%</span>
              </span>
              <span>{factor.score.toFixed(0)}/100</span>
            </div>
            <div className="bar">
              <div className="bar-fill" style={{ width: `${factor.score}%`, backgroundColor: scoreColour(factor.score) }} />
            </div>
            <div className="muted small">{factor.explanation}</div>
          </li>
        ))}
      </ul>
    </section>
  );
}

import { Card } from '../../shared/components/Card';
import { Button } from '../../shared/components/Button';
import type { CardData } from './chatTypes';

interface CardResultViewProps {
  data: CardData;
}

export function CardResultView({ data }: CardResultViewProps) {
  return (
    <div className="result-cards">
      <p className="result-meta">{data.totalRecords} record(s)</p>
      <div className="result-cards-grid">
        {data.cards.map((item, index) => (
          <Card
            key={`${item.title}-${index}`}
            title={item.title}
            subtitle={item.subtitle}
            footer={
              item.actions.length > 0 ? (
                <div className="card-actions">
                  {item.actions.map((action) => (
                    <Button key={action.action} variant="secondary">
                      {action.label}
                    </Button>
                  ))}
                </div>
              ) : undefined
            }
          >
            <dl className="field-list">
              {item.fields.map((field) => (
                <div key={field.label} className="field-row">
                  <dt>{field.label}</dt>
                  <dd>{field.value}</dd>
                </div>
              ))}
            </dl>
          </Card>
        ))}
      </div>
    </div>
  );
}

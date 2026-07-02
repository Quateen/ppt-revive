
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Reference } from '@/types/presentation';

interface ReferencesListProps {
  references: Reference[];
}

const ReferencesList: React.FC<ReferencesListProps> = ({ references }) => {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-xl">References</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {references.map((reference) => (
            <div
              key={reference.id}
              className="flex items-start gap-3 pb-3 border-b border-gray-100 last:border-0 last:pb-0"
            >
              <Badge
                variant="outline"
                className={
                  reference.type === 'journal'
                    ? 'bg-blue-50 text-blue-700 border-blue-200'
                    : reference.type === 'guideline'
                      ? 'bg-green-50 text-green-700 border-green-200'
                      : reference.type === 'review'
                        ? 'bg-purple-50 text-purple-700 border-purple-200'
                        : reference.type === 'meta-analysis'
                          ? 'bg-amber-50 text-amber-700 border-amber-200'
                          : 'bg-gray-50 text-gray-700 border-gray-200'
                }
              >
                {reference.type === 'meta-analysis'
                  ? 'Meta-Analysis'
                  : reference.type.charAt(0).toUpperCase() + reference.type.slice(1)}
              </Badge>
              <div className="flex-1 min-w-0">
                <p className="text-sm text-gray-800 line-clamp-2 break-words overflow-hidden">
                  {reference.link ? (
                    <a
                      href={reference.link}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-blue-700 underline hover:text-blue-900"
                      title={reference.citation}
                    >
                      <span className="inline-block truncate max-w-full">{reference.citation}</span>
                    </a>
                  ) : (
                    <span title={reference.citation} className="inline-block truncate max-w-full">
                      {reference.citation}
                    </span>
                  )}
                </p>
                <div className="mt-1">
                  <div className="flex items-center gap-2">
                    <span
                      className="text-xs font-medium text-medical-600 truncate max-w-full cursor-default"
                      title={reference.journal}
                    >
                      {reference.journal}
                    </span>
                  </div>
                  <div className="text-xs text-gray-500 mt-0.5 cursor-default">
                    {reference.year}
                  </div>
                </div>

              </div>
            </div>

          ))}
        </div>
      </CardContent>
    </Card>
  );
};

export default ReferencesList;

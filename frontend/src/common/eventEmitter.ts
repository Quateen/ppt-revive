// src/common/eventEmitter.ts
type EventCallback = () => void;

const eventMap = new Map<string, Set<EventCallback>>();

export const EventEmitter = {
  on: (event: string, callback: EventCallback) => {
    if (!eventMap.has(event)) {
      eventMap.set(event, new Set());
    }
    eventMap.get(event)!.add(callback);
  },

  off: (event: string, callback: EventCallback) => {
    eventMap.get(event)?.delete(callback);
  },

  emit: (event: string) => {
    eventMap.get(event)?.forEach((cb) => cb());
  },
};

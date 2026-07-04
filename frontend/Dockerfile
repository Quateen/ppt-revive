FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
# Vite bakes env vars into the bundle at build time
ARG VITE_API_BASE_URL
ARG VITE_GOOGLE_OAUTH_CLIENT_ID=
ARG VITE_FREE_TIER_SLIDES=5
ARG VITE_FOUNDING_URL=https://nucleusdigitalis.com
ARG VITE_PLAUSIBLE_DOMAIN=
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL \
    VITE_GOOGLE_OAUTH_CLIENT_ID=$VITE_GOOGLE_OAUTH_CLIENT_ID \
    VITE_FREE_TIER_SLIDES=$VITE_FREE_TIER_SLIDES \
    VITE_FOUNDING_URL=$VITE_FOUNDING_URL \
    VITE_PLAUSIBLE_DOMAIN=$VITE_PLAUSIBLE_DOMAIN
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_CHAT_HUB_URL=http://localhost:5046/chatHub
ENV VITE_CHAT_HUB_URL=$VITE_CHAT_HUB_URL

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

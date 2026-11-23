# ---- build Angular ----
FROM node:22 AS build
WORKDIR /app
COPY client/GesCreditoApp/package*.json ./
RUN npm ci
COPY client/GesCreditoApp/ ./
RUN npm run build

# ---- nginx ----
FROM nginx:1.27-alpine
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/GesCreditoApp/browser /usr/share/nginx/html
EXPOSE 80

FROM node:20-alpine
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install

COPY . .

EXPOSE 3000
ENV NODE_ENV=development
CMD ["node", "node_modules/vite/bin/vite.js", "--host", "0.0.0.0"]

FROM node:22-alpine AS node-builder
WORKDIR /backend
COPY package*.json ./
RUN npm install
COPY tsconfig.json ./
COPY src ./src
RUN npx tsc

FROM heroiclabs/nakama:3.39.0
COPY --from=node-builder /backend/build/index.js /nakama/data/modules/
COPY local.yml /nakama/data/

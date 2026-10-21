FROM node:22
WORKDIR /app
COPY . .
RUN npm install
# needed by src/edgegap.ts to stop the deployment
ENV EDGEGAP_API_TOKEN="token 1f3a9c2e-0000-4fake-8fake-000000000000"
EXPOSE 2567
CMD npm start

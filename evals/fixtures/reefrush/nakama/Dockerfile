FROM heroiclabs/nakama-pluginbuilder:3.39.0 AS builder
ENV GO111MODULE=on
ENV CGO_ENABLED=1
WORKDIR /backend
COPY go.mod ./
COPY *.go ./
RUN go mod tidy && go build --trimpath --buildmode=plugin -o ./backend.so .

FROM heroiclabs/nakama:3.39.0
COPY --from=builder /backend/backend.so /nakama/data/modules/

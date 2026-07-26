FROM alpine:3.19
COPY . /context
RUN ls -la /context/ | head -30

FROM alpine:3.19
COPY docker-publish/ /build
RUN du -sh /build && ls -la /build

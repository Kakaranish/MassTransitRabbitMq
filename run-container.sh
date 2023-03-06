#! /bin/bash

docker run --rm -it \
	-p 21380:80 \
	-e ASPNETCORE_ENVIRONMENT=Development \
	-e RabbitMq__Host=host.docker.internal \
	--name mt-rabbit \
	mt-rabbit

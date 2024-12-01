FROM node:20 as build-stage

ARG VITE_API_BACKEND_URL 

WORKDIR /app
COPY ./src/client_web/package.json ./
RUN yarn install
COPY ./src/client_web .

# COPY ./src/client_web/docker/substitute_environment_variables.sh /substitute_environment_variables.sh
# RUN chmod +x /substitute_environment_variables.sh
# CMD ["sh", "/substitute_environment_variables.sh"]

RUN yarn build

FROM nginx as production-stage
RUN mkdir /app
COPY --from=build-stage /app/dist /app
COPY ./src/client_web/src/assets/ /app/assets
COPY ./src/client_web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 8888
CMD ["nginx", "-g", "daemon off;"]

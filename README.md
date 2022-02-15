# Docker-Compose for [ELK stack](https://www.elastic.co/elastic-stack?ultron=B-Stack-Trials-AMER-US-C&gambit=Stack-ELK-Exact&blade=adwords-s&hulk=paid&Device=c&thor=elk%20stack&gclid=Cj0KCQiAmKiQBhClARIsAKtSj-lf1j3rNlwsu6tWZzoNYQGpnbrgNGGSEByo-38HHdHw3COSNEY93SQaAnOVEALw_wcB)

[Elasticsearch, Kibana, and Logstash (also known as ELK Stack)](https://www.elastic.co/elastic-stack?ultron=B-Stack-Trials-AMER-US-C&gambit=Stack-ELK-Exact&blade=adwords-s&hulk=paid&Device=c&thor=elk%20stack&gclid=Cj0KCQiAmKiQBhClARIsAKtSj-lf1j3rNlwsu6tWZzoNYQGpnbrgNGGSEByo-38HHdHw3COSNEY93SQaAnOVEALw_wcB) gives you the ability to aggregate logs from all your systems and applications, analyze these logs, and create visualizations for application and infrastructure monitoring, faster troubleshooting, security analytics, and more.

Here is a simple [docker-compose](https://github.com/ripplejb/ekl-stack/blob/master/docker-compose.yml) file to create the stack on docker. I have also added the rabbitmq to push data to the logstash.

1. Install [Docker](https://docs.docker.com/get-docker/).
1. Install [Docker-Compose](https://docs.docker.com/compose/).
1. Checkout the [repository](https://github.com/ripplejb/ekl-stack).
1. Run command `docker-compose up` to build and start the containers.
Once containers are running, 
1. Go to [http://localhost:15672](http://localhost:15672) to rabbitmq admin. 
1. Create a queue named `backup-log` as mentioned in the [logstash/pipeline/logstash.conf](https://github.com/ripplejb/ekl-stack/blob/master/logstash/pipeline/logstash.conf). 
1. Create an exchange named `app-logging` and bind it with the queue `backup-log` using route key named `backup-rk`.
1. Run the [sample .net application BackupDisks](https://github.com/ripplejb/ekl-stack/tree/master/BackupDisks): `BackupDisks /path/to/source/folder/ /path/to/destination/folder/`.
1. Go to Kibana [http://localhost:5601](http://localhost:5601).
1. [Create index pattern as described in the link and view your logs.](https://www.elastic.co/guide/en/kibana/7.17/index-patterns.html)


# Docker-Compose for [ELK stack](https://www.elastic.co/elastic-stack?ultron=B-Stack-Trials-AMER-US-C&gambit=Stack-ELK-Exact&blade=adwords-s&hulk=paid&Device=c&thor=elk%20stack&gclid=Cj0KCQiAmKiQBhClARIsAKtSj-lf1j3rNlwsu6tWZzoNYQGpnbrgNGGSEByo-38HHdHw3COSNEY93SQaAnOVEALw_wcB)

[Elasticsearch, Kibana, and Logstash (also known as ELK Stack)](https://www.elastic.co/elastic-stack?ultron=B-Stack-Trials-AMER-US-C&gambit=Stack-ELK-Exact&blade=adwords-s&hulk=paid&Device=c&thor=elk%20stack&gclid=Cj0KCQiAmKiQBhClARIsAKtSj-lf1j3rNlwsu6tWZzoNYQGpnbrgNGGSEByo-38HHdHw3COSNEY93SQaAnOVEALw_wcB) gives you the ability to aggregate logs from all your systems and applications, analyze these logs, and create visualizations for application and infrastructure monitoring, faster troubleshooting, security analytics, and more.

Here is a simple docker-compose file to create the stack on docker. I have also added the rabbitmq to push data to the logstash.

Before you begin, open the file `logstash/pipeline/ logstash.conf` and rename the queue and index as needed. See **TODO** in the JSON below.

Run command `docker-compose up` to build and start the containers.
Once containers are running, 
1. go to [http://localhost:15672](http://localhost:15672) to rabbitmq admin. 
2. create a queue with the same name as mentioned in the `logstash/pipeline/ logstash.conf`. See **TODO** in the JSON below.
 ```
input {
rabbitmq {
    id => "rabbit01"
    host => "rabbit01"
    port => 5672
    vhost => "/"
    queue => "backup"   // TODO: rename it to your queue name
    ack => false
  }}

## Add your filters / logstash plugins configuration here

output {
  elasticsearch {
    hosts => "es01:9200"
    index => "logstash_rabbit_mq_backup"   // TODO: rename it to your index name
  }
}
```
3. add some logs by passing some messages to queue.
4. go to Kibana [http://localhost:5601](http://localhost:5601).
5. [create index pattern as described in the link and view your logs.](https://www.elastic.co/guide/en/kibana/7.17/index-patterns.html)
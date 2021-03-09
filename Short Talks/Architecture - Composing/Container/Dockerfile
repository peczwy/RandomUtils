FROM tomcat
ADD tomcat-users.xml /usr/local/tomcat/conf/tomcat-users.xml
ADD manager.xml /usr/local/tomcat/conf/Catalina/localhost/manager.xml
CMD ["catalina.sh", "run"]
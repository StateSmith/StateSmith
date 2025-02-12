all: /tmp/plantuml.jar
	java -jar /tmp/plantuml.jar -headless -tsvg *.puml	

# This depends on graphviz
/tmp/plantuml.jar:
	wget https://github.com/plantuml/plantuml/releases/download/v1.2025.0/plantuml-asl-1.2025.0.jar -O /tmp/plantuml.jar


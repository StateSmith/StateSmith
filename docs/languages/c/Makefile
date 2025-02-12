# Assumes that ss.cli is in your path

all: /tmp/plantuml.jar
	java -jar /tmp/plantuml.jar -headless -tsvg lightbulb.puml	
	ss.cli run --lang=C99 --no-ask --no-csx -h -b

# This depends on graphviz
/tmp/plantuml.jar:
	wget https://sourceforge.net/projects/plantuml/files/plantuml.jar/download -O /tmp/plantuml.jar


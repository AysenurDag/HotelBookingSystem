from factory import create_app
from database import mongo
from rabbitmq_utils import consume_messages
from consumer import handle_message
import threading
import os
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.jaeger.thrift import JaegerExporter
from opentelemetry.instrumentation.flask import FlaskInstrumentor

def configure_tracing(app):
    trace.set_tracer_provider(TracerProvider())
    tracer = trace.get_tracer(__name__)

    jaeger_exporter = JaegerExporter(
        agent_host_name="jaeger",  # Docker kullanıyorsan "jaeger" olarak değiştir
        agent_port=6831,
    )

    span_processor = BatchSpanProcessor(jaeger_exporter)
    trace.get_tracer_provider().add_span_processor(span_processor)

    FlaskInstrumentor().instrument_app(app)

def start_consumer_thread(app):
    def start():
        with app.app_context():
            consume_messages("booking.reservation.created.queue", handle_message)
    threading.Thread(target=start, daemon=True).start()

if __name__ == '__main__':
    app = create_app(os.getenv('FLASK_ENV', 'development'))
    configure_tracing(app)
    start_consumer_thread(app)
    app.run(host='0.0.0.0', port=5050, debug=app.config['DEBUG'])
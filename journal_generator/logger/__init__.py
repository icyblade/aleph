import logging, os
from config import DEBUG_LEVEL

def configure_logger(file=False, stdout=True):
    logger = logging.getLogger(__name__)
    logger.setLevel(DEBUG_LEVEL)
    if file:
        if not os.path.exists('./log/'):
            os.mkdir('./log/')
        file_handler = logging.FileHandler(
            'log/%s.log' % os.path.basename(__file__)
        )
        file_handler.setLevel(DEBUG_LEVEL)
        file_handler.setFormatter(logging.Formatter(
            '%(asctime)s [%(levelname)s] %(funcName)s: %(message)s',
            '%H:%M:%S'
        ))
        logger.addHandler(file_handler)

    if stdout:
        stdout_handler = logging.StreamHandler()
        stdout_handler.setLevel(DEBUG_LEVEL)
        stdout_handler.setFormatter(logging.Formatter(
            '%(asctime)s [%(levelname)s] %(funcName)s: %(message)s',
            '%H:%M:%S'
        ))
        logger.addHandler(stdout_handler)
    return logger

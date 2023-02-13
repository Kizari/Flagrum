import time
from dataclasses import dataclass


@dataclass
class Timer:
    start: time

    def __init__(self):
        self.start = time.time()

    def print(self, process_name: str):
        print(f"{process_name} took {time.time() - self.start} seconds")
        self.start = time.time()

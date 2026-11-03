// WPF objects belong to the thread that made them, and the tests share one application: they all run on one thread.
[assembly: Apartment(ApartmentState.STA)]
